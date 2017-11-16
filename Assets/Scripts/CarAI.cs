using System.Collections.Generic;
using UnityEngine;

//npc 차의 상태
public enum State
{
    Accel, Brake, Curve
}

//차 npc 클래스. car 컴포넌트가 필수적으로 요구된다.
[RequireComponent(typeof(Car))]
public class CarAI : MonoBehaviour
{
    private Car car;
    private Transform tr;
    private Rigidbody rig;
    private List<Transform> nodes = new List<Transform>();  //1개의 차선을 갖는 Transform 타입의 리스트
    
    public GameObject Path; //처음 위치한 차선의 경로
    private float error = 1; //노드를 감지하는 거리.
    private float sensor = 4; //센서의 길이.
    public int speed { get { return (int)(rig.velocity.magnitude * 3.6f); } } //차의 속도.
    public int startNode; //처음에 향하는 노드.
    public int targetNode;//플레이중 향하는 노드.
    public State state = State.Accel; //npc의 초기 상태를 가속상태로 변경.

    private float steer; //앞 바퀴 회전각.

    private Vector3 original_pos; //AI의 초기 위치 상태
    private Quaternion original_rot; //AI의 초기 회전 상태

    //센서 관련 variables. 
    private Vector3[] sensorDirection; 
    private Vector3[] sensorPoint; 
    private byte[] isHit;
    public bool traffic_signal; //신호등 감지 여부 변수

    private float resultOfFront; //정면 감지 수치

    public void Start()
    {
        tr = GetComponent<Transform>();
        rig = GetComponent<Rigidbody>(); 
        car = GetComponent<Car>();
        traffic_signal = false; 


        if (car.Burn) //불 이펙트가 있으면 일단 비활성화 시킨다.
            car.Burn.SetActive(false);
        
        sensorDirection = new Vector3[3]; //정면, 왼쪽 40도, 오른쪽 40도
        sensorPoint = new Vector3[3]; //정면 가운데, 정면 왼쪽, 정면 오른쪽
        isHit = new byte[5]; //순서대로 정면 가운데, 정면 왼쪽, 정면 오른쪽, 왼쪽 대각선, 오른쪽 대각선

        nodes = Path.GetComponent<Path>().ListOfNodes; //할당된 경로를 List 형태로 변환하여 저장.
        if(nodes == null)
        {
            Debug.Log(gameObject.name + "안에 Path가 안들어있음");
        }
        startNode = (startNode < 0) ? 0 : startNode; //음수인 경우 0으로 맞춰준다.

        //startNode의 값이 해당 경로의 노드 개수보다 적으면 startNode를 그대로 targetNode로 사용하고, 
        //아니면 해당 경로의 마지막 노드를 targetNode로 사용한다.
        targetNode = (startNode < nodes.Count) ? startNode : nodes.Count - 1;

        
        SetOriginalTransform();
    }

    void FixedUpdate()
    {
        //센서 위치 설정 및 감지
        SensorPosition();
        GetSense();

        //현재 state에 따른 행동
        Action();

        //다음 state 설정
        SetNextState();
    }


    // 경로의 첫 노드를 얻어서 리스폰될 위치를 지정한다. //
    void SetOriginalTransform()
    {
        original_pos = gameObject.transform.position;
        original_rot = gameObject.transform.rotation;
    }

    // AI를 리스폰 위치로 되돌림. //
    public void PositionReset()
    {
        tr.position = original_pos;
        tr.rotation = original_rot;
        rig.velocity = 0 * rig.velocity.normalized; //속도를 0으로 바꾸어 초기화 후에 급발진 방지.
        car.steerAngle = 0; //스티어링도 초기화
        targetNode = startNode; //초기 타겟 노드
        state = State.Accel;
    }

    // state에 따른 행동 //
    void Action()
    {
        
       
        float v = 1;
        float brakeInput = 0;

        // 1. 타겟노드를 얻음
        // 2. 타겟노드와의 각도를 구하고 그에 대한 핸들 회전값을 구한다.
        GetTargetNode();
        GetSteer();

        // 3. 각 상태에 대한 Car.Move()의 입력값을 설정하고, Car.Move() 호출
        switch (state)
        {
            case State.Accel:
                car.SetMaxForwardSpeed(car.originalFSpeed); //가속 상태일때 속도 상한선을 설정해준 값으로 설정.
                break;
            case State.Curve: //커브 도는 동안에는 강제로 최대 속도의 1/4로 내려버림
                car.SetMaxForwardSpeed(car.originalFSpeed * 0.25f); //커브시 상한선을 낮추어 부드러운 커브 유도.
                break;
            case State.Brake: //브레이크 상태시 브레이크 입력 설정.
                v = 1; brakeInput = 1;
                break;
        }
        car.Move(v, steer, brakeInput, new bool[2] { false, true });
    }

    // targetNode에 대한 각도 구함 //
    void GetSteer()
    {
        //targetNode의 값이 현재 경로안에 포함이 된 경우에는
        //현재 차선에서의 targetNode에 해당하는 노드의 벡터값을 구해서
        //적용될 핸들 각도를 구한다.
        if (targetNode < nodes.Count)
        {
            Vector3 V = tr.InverseTransformPoint(nodes[targetNode].position); //차에 대한 다음 노드의 상대 좌표
            steer = V.x / V.magnitude; //적용될 핸들 각도
        }
        else steer = 0; //존재하지 않는 타겟노드에 대해선 방향전환을 하지 않는다.
    }

    // targetNode 설정 //
    void GetTargetNode()
    {
        //타겟노드와의 거리가 가까울 때 다음 타겟노드로 전환하는 것을 1순위로 생각함.
        //진로방해가 설정되면, 타겟노드를 재설정.
        try
        {
            float prevTargetDistance; //npc와 이전 노드와의 거리
            float currTargetDistance = Vector3.Distance(tr.position, nodes[targetNode].position); //npc와 현재 타겟 노드와의 거리

            if (targetNode < nodes.Count && (currTargetDistance < error))
            {
                targetNode++; //차와 노드와 거리 차가 error미만일 때, 다음 노드로 전환
                if (targetNode >= nodes.Count) // 차가 마지막 노드를 지났을 때, 초기 위치로 리젠시킨다.
                {
                    PositionReset();
                    return;
                }
            }
            else if (targetNode >= nodes.Count) //차가 마지막 노드를 지났을 때, 초기 위치로 리젠시킨다.
            {
                PositionReset();
                return;
            }
            //플레이어와 양 차선의 타겟노드와의 거리를 비교하여 더 길이가 짧은 쪽에 플레이어가 있다고 판단
            if (targetNode == 0)
                prevTargetDistance = Vector3.Distance(tr.position, car.original_pos);
            else
                prevTargetDistance = Vector3.Distance(tr.position, nodes[targetNode - 1].position);
            currTargetDistance = Vector3.Distance(tr.position, nodes[targetNode].position);

            //이전 노드보다 타겟 노드와의 거리가 가까울 경우에 타겟 노드 1증가
            if ((targetNode + 1 < nodes.Count) && (prevTargetDistance > currTargetDistance))
            {
                targetNode += 1;
            }
        }
        catch(System.ArgumentOutOfRangeException e) //범위를 벗어난 참조 관련 예외처리
        {
            Debug.Log(e.StackTrace);
            PositionReset();
            return;
        }
        
    }
   
    // 다음 state 설정 //
    void SetNextState()
    {
        //센서 각각에 가중치를 부여하여 전방 감지 결과를 얻음.
        resultOfFront = 1f - (1.3f * isHit[0] + 0.3f * isHit[1] + 0.3f * isHit[2] + 0.05f * isHit[3] + 0.05f * isHit[4]);

        //전방 오브젝트를 감지하지 못했다고 판단되거나 신호에 걸리지 않은 경우
        if ((targetNode < nodes.Count) && resultOfFront > 0 && traffic_signal == false)
        {
            try
            {
                if (nodes[targetNode].CompareTag("slide")) //커브구간인 경우 커브 상태로 변경.
                {
                    state = State.Curve;
                }
                else //커브가 아닌 경우 가속 상태로 변경.
                {
                    state = State.Accel;  
                }
            }
            catch(System.ArgumentOutOfRangeException e)
            {
                Debug.Log(e.StackTrace);
                PositionReset();
                return;
            }
        }
        else //전방 오브젝트가 감지되어 resultOfFront의 값이 음수가 된 경우나 신호에 걸린 경우 브레이크
        {
            state = State.Brake;
        }
    }

    // 센서 위치 및 방향, 충돌 여부 갱신 //
    void SensorPosition()
    {
        //센서 시작 좌표 초기화
        sensorPoint[0] = tr.TransformPoint(new Vector3(0, 0.5f, 2.454f));
        sensorPoint[1] = tr.TransformPoint(new Vector3(-0.781f, 0.5f, 2.31f));
        sensorPoint[2] = tr.TransformPoint(new Vector3(0.781f, 0.5f, 2.31f));

        //센서 방향 초기화
        sensorDirection[0] = tr.TransformDirection(Vector3.forward);
        sensorDirection[1] = tr.TransformDirection(new Vector3(-0.642f, 0, 0.766f));
        sensorDirection[2] = tr.TransformDirection(new Vector3(0.642f, 0, 0.766f));

        //충돌 변수 초기화
        for (int i=0; i<5; i++)
            isHit[i] = 0;

    }

    // 센서 감지 여부 확인 //
    void GetSense()
    {
        RaycastHit hit;
        for (int i = 0; i < 5; i++)
        {
            Vector3 direction = (i < 3) ? sensorDirection[0] : sensorDirection[i - 2];
            Vector3 point = (i < 3) ? sensorPoint[i] : sensorPoint[i - 2];

            //전방 센서 = 2x sensor, 대각선 = sensor
            if (Physics.Raycast(point, direction, out hit, (i < 3) ? 2 * sensor :sensor))
            {
                if (!hit.collider.isTrigger) //트리거 콜라이더는 센서 감지에서 제외
                {
                    if (hit.collider.CompareTag("AICar")) //충돌체 중에서 AI만 인식
                    {
                        Debug.DrawLine(point, hit.point, Color.white);
                        isHit[i] = 1;
                    }
                }
            }
        }
    }


    // 신호등 감지 영역에 있는 경우 //
    private void OnTriggerStay(Collider other)
    {

        if (other.gameObject.CompareTag("trafficsign"))
        {

            //신호등 인식 영역에 진입한 것에 대한 코드 작성
            TrafficLight traffic_light;
            if ((traffic_light = other.gameObject.GetComponent<TrafficController>()) == null)
            {
                traffic_light = other.gameObject.GetComponent<TrafficController2>();
            }

            //신호등이 빨간 불인가?
            if (traffic_light.lights[0].intensity == traffic_light.inten_light)
                traffic_signal = true;
            else
                traffic_signal = false;

        }

    }

    // AI가 신호영역에서 벗어난 경우 traffic_signal을 false로 전환 //
    private void OnTriggerExit(Collider other)
    {

        if (other.gameObject.CompareTag("trafficsign"))
            traffic_signal = false;
    }

}
