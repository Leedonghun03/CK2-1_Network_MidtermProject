using UnityEngine;

public class OtherPlayerCar : MonoBehaviour
{
    float moveSpeed = 5.0f;
    float OtherCarXPos;

    // 네트워크 관련 필요한 필드 추가
    int networkFlag = 0;

    void Start()
    {
        OtherCarXPos = transform.position.x;
    }

    void Update()
    {
        if (networkFlag == 1)
        {
            Vector2 target = new Vector2(OtherCarXPos, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, target, Time.deltaTime * moveSpeed);

            if (Mathf.Abs(transform.position.x - OtherCarXPos) < 0.01f)
            {
                transform.position = target;
                networkFlag = 0;
            }
        }
    }

    public void SetTargetPosition(float posX)
    {
        networkFlag = 1;
        OtherCarXPos = posX;
    }
}
