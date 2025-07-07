using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CarControllor : MonoBehaviour
{
    // 스피드, 초기 위치 변수
    float speed = 0.0f;
    Vector2 startPos;

    public bool isGameOver = false;

    // 네트워크 관련 필요한 필드 추가
    GameObject car;
    GameObject flag;
    int networkFlag = 0;

    // 네트워크 전달 데이터
    float length;
    Vector2 carPos;

    void Start()
    {
        Application.targetFrameRate = 60;
        this.car = GameObject.Find("car_0");
        this.flag = GameObject.Find("flag_0");
    }

    void Update()
    {
        if(isGameOver)
        {
            if(transform.position.x > 8.0f)
            {
                speed = 0.0f;
            }

            return;
        }
            
        if (Input.GetMouseButtonDown(0))
        {
            this.startPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector2 endPos = Input.mousePosition;

            if(startPos.x < endPos.x)
            {
                float swipeLength = endPos.x - startPos.x;

                // 기획자에게 정해보라고 하세요.
                this.speed = swipeLength / 500.0f;

                // 효과음을 재생해요.
                GetComponent<AudioSource>().Play();
                this.networkFlag = 1;
            }
        }

        transform.Translate(this.speed, 0, 0);
        
        // 천천히 멈추게 해요.
        this.speed *= 0.96f;

        // 네트워크로 정보 전송
        if(this.speed < 0.0001f && networkFlag == 1)
        {
            // 전달 정보 준비
            length = this.flag.transform.position.x - this.car.transform.position.x;
            if (length < 0)
            {
                length = -99.99f;
            }
            
            carPos = transform.position;

            ClientManager.Instance.SendMyPosition(length, carPos);
            networkFlag = 0;
        }
    }
}
