using UnityEngine;

//플레이어가 키보드로 입력하는 것에 대한 클래스. 이 클래스를 갖는 오브젝트는 Car 클래스 타입의 컴포넌트를 가지고 있어야 한다.
[RequireComponent (typeof(Car))]
public class UserController : MonoBehaviour {

    //플레이어의 입력값을 얻는 구조체
    private struct CarUserInput
    {
        public float HorizontalInput; //수평입력
        public float VerticalInput; //수직입력
        public float BrakeInput; //브레이크 입력
        public bool[] GearInput; //기어입력방식 (핸들 밑 좌우 버튼)
        public bool[] OtherInput; //추후 필요한 그외 입력버튼들.(운전 자체에 영향을 주지 않는 버튼 == Move()에 미포함 되는 버튼)
        /* OtherInput
         * 0 : 위치초기화(End)
         * 그외 추가 버튼 필요시 이 배열에 추가하여 작업
         */
    };

    private CarUserInput userInput;
    private Car Player;
    
    //씬이 로드되었을 때 호출. start()보다 먼저 호출됨.
    private void Awake()
    {
        Player = GetComponent<Car>();
    }


    void Start () {
        userInput.GearInput = new bool[2];
        userInput.OtherInput = new bool[1];
    }

    //고정된 시간마다 주기적으로 호출되는 함수
    private void FixedUpdate()
    {
        //일시 정지가 아닐 때에만 키 입력을 받도록 한다.
        userInput.HorizontalInput = (SceneArgument.isPause == false) ? Input.GetAxis("Horizontal") : 0; //좌우 방향키
        userInput.VerticalInput = (SceneArgument.isPause == false) ? Input.GetAxis("Vertical") : 0; //위 아래 방향키
        userInput.BrakeInput = (SceneArgument.isPause == false) ? Input.GetAxis("Jump") : 0; //스페이스 버튼

        //gear = R, N, D (후진, 중립, 전진)
        userInput.GearInput[1] = (SceneArgument.isPause == false) ? Input.GetKeyDown(KeyCode.X) : userInput.GearInput[1]; // X : 기어 올림
        userInput.GearInput[0] = (SceneArgument.isPause == false) ? Input.GetKeyDown(KeyCode.Z) : userInput.GearInput[0]; // Z : 기어 내림

       
        userInput.OtherInput[0] = (SceneArgument.isPause == false) ? Input.GetKeyDown(KeyCode.End) : userInput.OtherInput[0]; //위치 초기화 하고싶을 때 end 누름

        Player.Move(userInput.VerticalInput, userInput.HorizontalInput, userInput.BrakeInput, userInput.GearInput);
        Player.PositionReset(userInput.OtherInput[0]); //위치 초기화 입력시 인자 넘겨줌
        
    }
}
