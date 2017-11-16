using System.Collections;
using UnityEngine;

//Car 클래스를 분할하여 작성한 것.
public partial class Car : MonoBehaviour
{
    public GameObject Burn; //폭발 이펙트 오브젝트
    public GameObject HitSound; //충돌 사운드 오브젝트

    [SerializeField]
    private AudioClip AccelerationLow; //엔진소리 클립
    private AudioSource audioSource; //사운드 출력시 필요한 컴포넌트.

    //엔진 소리 초기화 함수.
    void SetEngineSound()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = AccelerationLow;
        audioSource.loop = true;
        audioSource.volume = 1;
        audioSource.Play();
    }

    //속도에 따른 엔진 소리 피치 조절
    void UpdateEngineSound()
    {
        /*
         * [0, 1]를 만족하는 x에 대하여 1 - (1-x)^2는 [0, 1]를 만족한다.
         * 증가함수이나 기울기가 감소하는 그래프이다.
         */
        audioSource.pitch = 2.5f * (1 - (1 - speed / MaxForwardSpeed) * (1 - speed / MaxForwardSpeed));
    }

    // 물리적 충돌이 발생할 때 누구와 충돌했는지 알려주는 함수
    private void OnCollisionEnter(Collision other)
    {
        if(gameObject.CompareTag("Player")) //플레이어가 AI나 건물에 충돌시 효과음 발생
        {
            if (other.gameObject.CompareTag("AICar") || other.gameObject.CompareTag("building"))
            {
                GameObject sound_effect = Instantiate(HitSound, transform.position, transform.rotation);
                Destroy(sound_effect, 1);
            }
            
        }
        else if(gameObject.CompareTag("AICar")) //AI가 도로를 제외한 그외의 것과 충돌시 리스폰을 기다리도록 함
        {
            if(other.gameObject.CompareTag("Roads") == false)
            {
                //위의 조건을 만족한 충돌시에 폭발 이펙트 활성화.
                if(Burn.activeSelf == false)
                {
                    Burn.SetActive(true);
                    StartCoroutine(PushBurningCar());
                }
                
            }
        }
    }
    
    //충돌한 차를 2초~10초 후에 리스폰하는 coroutine
    private IEnumerator PushBurningCar()
    {
        CarAI car_ai = gameObject.GetComponent<CarAI>();
        Random.InitState((int)System.DateTime.Now.Ticks);
        float respawn_time = Random.Range(2, 10); //2~10 범위 내에서 랜덤값 복사
        yield return new WaitForSeconds(respawn_time); //위의 값만큼 대기.
        Burn.SetActive(false); //대기 완료 후에 폭발 이펙트 비활성화.
        car_ai.PositionReset(); //해당하는 npc 리스폰
    }

    //위치 초기화 키를 누른 경우 차의 위치를 초기화하는 함수.
    //(UserController에 있어야할 함수이지만 코딩 편의성을 위해 여기에 작성.)
    public void PositionReset(bool PositionInput) //위치 초기화
    {
        if (PositionInput)
        {
            rig.velocity = 0 * rig.velocity.normalized;
            transform.localPosition = original_pos; //초기 위치
            transform.localRotation = original_rot; //초기 회전
        }
    }
}
