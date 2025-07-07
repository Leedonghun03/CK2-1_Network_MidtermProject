using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CarControllor : MonoBehaviour
{
    // ���ǵ�, �ʱ� ��ġ ����
    float speed = 0.0f;
    Vector2 startPos;

    public bool isGameOver = false;

    // ��Ʈ��ũ ���� �ʿ��� �ʵ� �߰�
    GameObject car;
    GameObject flag;
    int networkFlag = 0;

    // ��Ʈ��ũ ���� ������
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

                // ��ȹ�ڿ��� ���غ���� �ϼ���.
                this.speed = swipeLength / 500.0f;

                // ȿ������ ����ؿ�.
                GetComponent<AudioSource>().Play();
                this.networkFlag = 1;
            }
        }

        transform.Translate(this.speed, 0, 0);
        
        // õõ�� ���߰� �ؿ�.
        this.speed *= 0.96f;

        // ��Ʈ��ũ�� ���� ����
        if(this.speed < 0.0001f && networkFlag == 1)
        {
            // ���� ���� �غ�
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
