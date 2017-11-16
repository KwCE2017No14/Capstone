using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public partial class Car : MonoBehaviour
{
    [SerializeField] private Vector3 centerOfMass; //무게중심
    //[SerializeField] private float lowSpeedSteerAngle = 0.1f; //최소 스티어링 각도
    [SerializeField] private float highSpeedSteerAngle = 40f; //최고 스티어링 각도 [SerializeField]
    [SerializeField] private float MaxForwardSpeed = 80f; //최고 전진 속도
    [SerializeField] private float MaxBackwardSpeed = 70f; //최고 후진 속도
    [SerializeField] private float ForwardTorque = 100000; //전진할 때 토크
    [SerializeField] private float BackwardTorque = 35000; //후진할 때 토크
    [SerializeField] private float BrakeTorque = 50000; //브레이크시 토크
    [SerializeField] private float downforce = 100f; //다운 포스
    
    [SerializeField] private WheelCollider[] wheelColliders = new WheelCollider[4]; //휠 콜라이더
    [SerializeField] private Transform[] tireMeshes = new Transform[4]; //타이어 메쉬의 트랜스폼
    [SerializeField] private GameObject[] lights = new GameObject[4]; //BrakeR, BrakeL, BackwardR, BackwardL

    private Rigidbody rig; //차의 리지드바디
    public float speed { get; private set; } //차의 속도
    //public float steerAngle { get; set; } //스티어링 각도
    public float steerAngle;
//    public bool PreemptiveSteering { get; set; } //스티어링 권한. 외부에서 강제로 권한을 뺏은 경우 true.
    public float torque;
    public float originalFSpeed { get; private set; } //초기 최대 속도
    private int BeforeGear; //이전 기어
    private int gear; //기어 변수 (R, N, D, 이전)

    public Vector3 original_pos { get; private set; } //AI의 초기 위치 상태
    public Quaternion original_rot { get; private set; } //AI의 초기 회전 상태

    private void Awake()
    {
        rig = GetComponent<Rigidbody>();
        rig.centerOfMass = centerOfMass; //무게중심 설정
        originalFSpeed = MaxForwardSpeed;
        gear = 1; //중립 기어로 초기화
        SetEngineSound(); //엔진소리 초기화
        original_pos = GetComponent<Transform>().localPosition;
        original_rot = GetComponent<Transform>().localRotation;
    }

    private void Update()
    {
        UpdateEngineSound(); //엔진소리 업데이트
        UpdateTireMeshes(); //타이어 메쉬 업데이트
    }

    //차의 이동
    public void Move(float v, float h, float brakeInput, bool[] gearInput)
    {
        //액셀, 브레이크 입력 값 클램핑. 음수는 사용하지 않는다.
        float Movement = Mathf.Clamp(v, 0, 1); //악셀
        float Brake = Mathf.Clamp(brakeInput, 0, 1); //브레이크
        AutoGearChange();
        GearType(gearInput);
        Braking(brakeInput);
        BackLight(v, Brake);

        if (SceneArgument.isPause == false)
        {
            if ((Movement > 0) && (speed < MaxForwardSpeed) && gear == 2) //전진
            {
                torque = wheelColliders[2].motorTorque = Movement * ForwardTorque * Time.deltaTime;
                wheelColliders[3].motorTorque = Movement * ForwardTorque * Time.deltaTime;
            }
            else if ((Movement > 0) && (speed < MaxBackwardSpeed) && gear == 0) //후진
            {
                torque = wheelColliders[2].motorTorque = -Movement * BackwardTorque * Time.deltaTime;
                wheelColliders[3].motorTorque = -Movement * BackwardTorque * Time.deltaTime;
            }
            else //엑셀 누르지 않으면 속도 저하 (엔진 브레이크)
            {
                if ((speed >= 1) && (gear == 2))
                {
                    if (Vector3.Dot(transform.forward, rig.velocity) > 0)
                    {
                        torque = wheelColliders[2].motorTorque = wheelColliders[3].motorTorque = - 500;
                    }
                    else
                    {
                        torque = wheelColliders[2].motorTorque = wheelColliders[3].motorTorque = 0;
                    }
                }
                else if ((speed >= 1) && (gear == 0))
                {
                    if (Vector3.Dot(transform.forward, rig.velocity) < 0)
                    {
                        torque = wheelColliders[2].motorTorque = wheelColliders[3].motorTorque = 500;
                    }
                    else
                    {
                        torque = wheelColliders[2].motorTorque = wheelColliders[3].motorTorque = 0;
                    }
                }
                else
                {
                    wheelColliders[2].brakeTorque = Mathf.Infinity;
                    wheelColliders[3].brakeTorque = Mathf.Infinity;
                }
            }
        }
        else
        {
            wheelColliders[2].motorTorque = wheelColliders[3].motorTorque = 0;
            wheelColliders[2].brakeTorque = wheelColliders[3].brakeTorque = Mathf.Infinity;
        }
        //스티어링 각도 적용
        // 속도에 따라 방향전환율을 달리 적용하기 위한 계산
        /*
           * Mathf.Lerp(from, to, t) : Linear Interpolation(선형보간)
           * from:시작값, to:끝값, t:중간값(0.0 ~ 1.0)
           * t가 0이면 from을 리턴, t가 1이면 to 를 리턴함, 0.5라면 from, to 의 중간값이 리턴
           * return = (1-t) * from + t * to
       */
        //float speedFactor = GetComponent<Rigidbody>().velocity.magnitude / MaxForwardSpeed;
        //float steerAngle = Mathf.Lerp(lowSpeedSteerAngle, highSpeedSteerAngle, 1 / speedFactor)
//        if (PreemptiveSteering == false)
//        {
            steerAngle = highSpeedSteerAngle * h;
            //print("steerAngle:" + steerAngle);
//        }
        wheelColliders[0].steerAngle = steerAngle;
        wheelColliders[1].steerAngle = steerAngle;

        //속도 클램핑(범위를 초과하는 경우 범위 이내로 값을 잘라낸다.)
        speed = rig.velocity.magnitude * 3.6f;
        if (SceneArgument.isPause == false)
        {
            if (gear == 2 && speed > MaxForwardSpeed) //전진기어. 최고속도에서 클램핑
                rig.velocity = MaxForwardSpeed / 3.6f * rig.velocity.normalized;
            else if (gear == 0 && speed > MaxBackwardSpeed) //후진기어. 50km/h에서 클램핑
                rig.velocity = MaxBackwardSpeed / 3.6f * rig.velocity.normalized;
        }
        else
        {
            rig.velocity = Vector3.zero;
        }
        //다운포스를 적용한다. 다운포스의 크기는 차의 속력에 비례한다.
        rig.AddForce(-Vector3.up * downforce * rig.velocity.magnitude);
    }

    //임의로 최대속도를 변경하는데 사용.
    public void SetMaxForwardSpeed(float velocity)
    {
        MaxForwardSpeed = velocity;
    }

    //기어 입력에 대하여 기어 타입을 변화시킨다. (R, N, D 순)
    void GearType(bool[] gearInput)
    {
        if (((speed < 0.5f || BeforeGear == 0) && gear == 1 && gearInput[0]))
        { //속도가 0.5미만 이거나 이전 기어 상태가 R일때, 현재 기어가 N이고 S키를 눌렀다면
            gear = 0;
        }
        else if ((gear == 0 && gearInput[1]) || (gear == 2 && gearInput[0]))
        { //현재 기어가 R이고 W키를 눌렀거나 현재 기어가 D이고 S키를 눌렀다면
            BeforeGear = gear; //이전 기어(R 이나 D) 정보를 백업함.
            gear = 1;
        }
        else if ((speed < 0.5f || BeforeGear == 2) && gear == 1 && gearInput[1])
        { //속도가 0.5미만 이거나 이전 기어 상태가 D일때, 현재 기어가 N이고 W키를 눌렀다면
            gear = 2;
        }
    }

    //자동으로 기어의 단수를 변화시킨다.
    void AutoGearChange()
    {
        int gearFactor = 0; //내부적으로 계산되는 기어의 레벨.
        if (gear == 2) //D 기어일 때
        {
            if (speed <= 20f) // 기어 1단
                gearFactor = 0;
            else if (speed > 20f && speed <= 30f) // 기어 2단
                gearFactor = 1;
            else if (speed > 30f && speed <= 40f) // 기어 3단
                gearFactor = 2;
            else if (speed > 40f && speed <= 50f) // 기어 4단
                gearFactor = 3;
            else// 기어 5단
                gearFactor = 4;
        }
        else //다른 기어 일때, gearFactor = 0으로 고정
            gearFactor = 0;

        ForwardTorque = 100000 - 10000 * gearFactor; //기어가 올라갈 때 마다 휠의 토크를 감소시킨다.
    }

    //브레이크 입력을 받아 작동시킨다.
    void Braking(float brakeInput)
    {
        if (brakeInput > 0) // GetKeyDown은 Space, GetButton은 미리 정의된 key
        {
            wheelColliders[0].motorTorque = 0;
            wheelColliders[1].motorTorque = 0;
            for (int i = 0; i < 4; i++)
                wheelColliders[i].brakeTorque = 1.5f * BrakeTorque * brakeInput;
        }
        else
        {
            for (int i = 0; i < 4; i++)
                wheelColliders[i].brakeTorque = 0;
        }
    }

    //차 뒤의 라이트 관련
    void BackLight(float v, float brake)
    {
        if (speed >= 0 && v > 0 && gear == 2) // 전진시
        {
            lights[0].SetActive(false);
            lights[1].SetActive(false);
            lights[2].SetActive(false);
            lights[3].SetActive(false);
        }
        else if (speed >= 0 && v > 0 && gear == 0) // 후진시
        {
            lights[0].SetActive(false);
            lights[1].SetActive(false);
            lights[2].SetActive(true);
            lights[3].SetActive(true);
        }
        if (brake > 0 || gear == 1) // 브레이크 작동시
        {
            lights[0].SetActive(true);
            lights[1].SetActive(true);
            lights[2].SetActive(false);
            lights[3].SetActive(false);
        }
    }

    //휠의 움직임에 대해 메쉬를 똑같이 맞춰준다.
    void UpdateTireMeshes()
    {
        //wheel collider의 위치와 쿼터니언 값을 받아 메쉬에 똑같이 적용시킨다.
        for (int i = 0; i < 4; i++)
        {
            Quaternion quat;
            Vector3 pos;
            wheelColliders[i].GetWorldPose(out pos, out quat);

            tireMeshes[i].position = pos;
            tireMeshes[i].rotation = quat;
        }
    }
}

