using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//도시나 고속도로 씬 상에서 우측 상단에 있는 메뉴 및 그외 UI관련 클래스
public class InGameMenu : MonoBehaviour
{
    public Vector3 startingPoint; //플레이어 시작 위치
    public GameObject[] playerModel = new GameObject[3]; //0:플레이어 부모 오브젝트, 1:승용차, 2:트럭
    public Text speedometer; //속도계
    public GameObject menu; //메뉴
    public GameObject map; //미니맵
    private GameObject player; //플레이어

    // Use this for initialization
    void Awake()
    {
        player = playerModel[SceneArgument.numOfCar]; //메인화면에서 결정한 차 모델로 플레이어 객체 할당.
        startingPoint = player.transform.position; //초기위치 설정

        //선택한 모델만 활성화시키고 나머지 하나는 비활성화
        playerModel[(SceneArgument.numOfCar % 2) + 1].SetActive(false);
        playerModel[SceneArgument.numOfCar].SetActive(true);

        //속도계 객체 할당.
        if (!speedometer)
        {
            GameObject obj = GameObject.Find("Speedometer text");
            if (obj)
                speedometer = obj.GetComponent<Text>();
        }

        //미니맵 객체 할당
        if (!map)
        {
            GameObject obj = GameObject.Find("Map");
            if (obj)
            {
                map = obj;
                map.transform.position = startingPoint + new Vector3(0, 10, 0);
            }

        }

        menu.SetActive(false); //시작시 메뉴창 비활성화
    }

    //update 함수 중에서 마지막에 호출되는 update함수.
    private void LateUpdate()
    {
        int speed = (int)player.GetComponent<Car>().speed; //플레이어의 속도값 복사.
        speedometer.text = speed.ToString() + "km/h"; //속도계에 속도 출력.
        map.transform.position = player.transform.position + new Vector3(0, 10, 0);
    }

    // 메뉴 창 on, off 함수
    public void OnOffMenu()
    {
        //메뉴창 활성화시 차들을 정지
        if (SceneArgument.isPause == false)
        {
            SceneArgument.isPause = true;
            menu.SetActive(true);
        }
        else //메뉴창 비활성화시 다시 움직임.
        {
            SceneArgument.isPause = false;
            menu.SetActive(false);
        }
    }
    
    //메뉴창 안에 있는 exit 버튼 함수
    public void Btn_Exit()
    {
        //종료 버튼
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                            Application.Quit();
#endif

    }

    //메뉴창 안에 있는 title 버튼 함수
    public void Btn_To_Title()
    {
        SceneManager.LoadScene("Main"); //main scene 로드
    }

}



