using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//모든 씬에서 공유되는 정적 클래스
public static class SceneArgument
{
    public static bool isPause = false; //일시 정지 여부
    public static int numOfCar = 1; //유저가 선택한 차의 종류(1:승용차, 2:트럭)
}

//메인화면에서 사용되는 클래스
public class main : MonoBehaviour {
    private struct Flags //main scene에서 사용되는 플래그
    {
        public int opt; //맵 번호 (0:도시, 1:고속도로)
        public uint num; //메인화면 순서 (0:첫 화면, 1:차 선택 화면, 2:맵 선택 화면, 3:플레이 화면)
    };

    public Text[] text = new Text[8];
    public Image[] image = new Image[4];
    public GameObject[] carGroup = new GameObject[3];

    private Flags flags;
    public int nn;
	// 초기화
	void Start () {
        flags.opt = 0;
        flags.num = 0;

        text[0].gameObject.SetActive(true);
        text[1].gameObject.SetActive(true);
        text[2].gameObject.SetActive(false);
        text[3].gameObject.SetActive(false);
        text[4].gameObject.SetActive(false);
        text[5].gameObject.SetActive(false);
        text[6].gameObject.SetActive(false);
        

        carGroup[0].SetActive(false);
        carGroup[1].SetActive(true);
        carGroup[2].SetActive(false);
        image[0].gameObject.SetActive(false);
        image[1].gameObject.SetActive(false);
        image[2].gameObject.SetActive(false);
        image[3].gameObject.SetActive(false);
    }

    // 매 프레임마다 호출
    void Update()
    {
        carGroup[0].transform.Rotate(0, 2, 0); //차 모델을 매 프레임마다 회전
        nn = flags.opt;
    }

    //다음 Scene 불러오기
    public IEnumerator StartLoadScene()
    {
        int loading = 0;
        //flags.opt 값에 따라 씬을 로드
        AsyncOperation asyncLoad = (flags.opt == 0) ? SceneManager.LoadSceneAsync("City") : SceneManager.LoadSceneAsync("Highway");
        asyncLoad.allowSceneActivation = false;
       
        //로딩 상황에 따라 로딩 메시지 변경 
        while (asyncLoad.progress < 0.9f)
        {
            text[6].text = "Loading";
            for (int i = 0; i < loading; i++)
                text[6].text += ".";
            yield return new WaitForSeconds(0.5f);
            loading = (loading + 1) % 4;
        }
        asyncLoad.allowSceneActivation = true;
        yield return null;
    }

    //main scene의 start 버튼을 클릭하였을 때 호출되는 함수
    public void Btn_Start()
    {
        flags.num = 1; //차 선택화면으로 넘어감
        text[0].gameObject.SetActive(false);
        text[1].gameObject.SetActive(false);
        text[2].gameObject.SetActive(true);
        text[3].gameObject.SetActive(true);
        text[4].gameObject.SetActive(true);
        text[5].gameObject.SetActive(true);
        text[6].gameObject.SetActive(false);

        carGroup[0].SetActive(true);
        carGroup[1].SetActive(true);
        carGroup[2].SetActive(false);
        SceneArgument.numOfCar = 1;

    }

    //main scene의 exit 버튼을 클릭하였을 때 호출되는 함수
    public void Btn_End()
    {
#if UNITY_EDITOR //유니티 에디터인 경우 에디터 상에서 실행한 것을 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else //빌드하여 생성된 exe파일을 통해서 실행된 경우 해당 프로그램 종료
                            Application.Quit();
#endif
    }

    //main scene의 back 버튼을 클릭하였을 때 호출되는 함수
    public void Btn_Back()
    {
        switch(flags.num)
        {
            case 1: //차 선택화면에서 back 버튼을 클릭한 경우 첫 화면으로 전환
                flags.num = 0;
                SceneArgument.numOfCar = 1;
                carGroup[0].SetActive(false);
                carGroup[1].SetActive(true);
                carGroup[2].SetActive(false);
                text[0].gameObject.SetActive(true);
                text[1].gameObject.SetActive(true);
                text[2].gameObject.SetActive(false);
                text[3].gameObject.SetActive(false);
                text[4].gameObject.SetActive(false);
                text[5].gameObject.SetActive(false);
                text[6].gameObject.SetActive(false);

                break;
            case 2: //맵 선택화면에서 back 버튼을 클릭한 경우 차 선택 화면으로 전환
                flags.num = 1;
                flags.opt = 0;
                image[0].gameObject.SetActive(false);
                image[1].gameObject.SetActive(false);
                image[2].gameObject.SetActive(false);
                image[3].gameObject.SetActive(false);
                carGroup[0].SetActive(true);
                text[0].gameObject.SetActive(false);
                text[1].gameObject.SetActive(false);
                text[2].gameObject.SetActive(true);
                text[3].gameObject.SetActive(true);
                text[4].gameObject.SetActive(true);
                text[5].gameObject.SetActive(true);
                text[6].gameObject.SetActive(false);
                break;
        }
    }

    //main scene의 confirm 버튼을 클릭하였을 때 호출되는 함수
    public void Btn_Confirm()
    {
        if(flags.num == 1) //차 선택 화면인 경우 맵 선택화면으로 전환
        {
            flags.num++;
            text[0].gameObject.SetActive(false);
            text[1].gameObject.SetActive(false);
            text[2].gameObject.SetActive(true);
            text[3].gameObject.SetActive(true);
            text[4].gameObject.SetActive(true);
            text[5].gameObject.SetActive(true);
            text[6].gameObject.SetActive(false);

            carGroup[0].SetActive(false);
            image[0].gameObject.SetActive(true);
            image[1].gameObject.SetActive(true);
            image[2].gameObject.SetActive(false);
            image[3].gameObject.SetActive(false);

        }
        else if(flags.num == 2) //맵 선택 화면인 경우 선택한 맵으로 씬 전환
        {
            flags.num++; //flags.num == 3 : 본 플레이 화면
            text[6].gameObject.SetActive(true);
            StartCoroutine(StartLoadScene());
        }
    }

    //main scene의 화살표 버튼을 클릭하였을 때 호출되는 함수
    public void Btn_Arrow()
    {
        if (flags.num == 1) //차 선택 화면인 경우 다음 차로 변경
        {
            carGroup[SceneArgument.numOfCar].SetActive(false);
            SceneArgument.numOfCar = (SceneArgument.numOfCar % 2) + 1;
            carGroup[SceneArgument.numOfCar].SetActive(true);
        }
        else if(flags.num == 2) //맵 선택 화면인 경우 다음 맵으로 변경
        {
            flags.opt = (flags.opt + 1) % 2;
            switch(flags.opt)
            {
                case 0: //도시 맵
                    image[0].gameObject.SetActive(true);
                    image[1].gameObject.SetActive(true);
                    image[2].gameObject.SetActive(false);
                    image[3].gameObject.SetActive(false);
                    break;
                case 1: //고속도로 맵
                    image[0].gameObject.SetActive(false);
                    image[1].gameObject.SetActive(false);
                    image[2].gameObject.SetActive(true);
                    image[3].gameObject.SetActive(true);
                    break;
            }
        }
    }
}
