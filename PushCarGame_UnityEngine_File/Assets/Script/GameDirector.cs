using UnityEngine;
using TMPro;

public class GameDirector : MonoBehaviour
{
    GameObject car;
    GameObject flag;
    GameObject distance;
    GameObject timer;
    GameObject rankPanel;
    GameObject rankText;

    private float time;
    private bool isFinishSend = false;

    void Start()
    {
        this.car = GameObject.Find("car_0");
        this.flag = GameObject.Find("flag_0");
        this.distance = GameObject.Find("Distance");
        this.timer = GameObject.Find("Timer");

        this.rankText = GameObject.Find("RankText");
        this.rankPanel = GameObject.Find("RankPanel");
        this.rankPanel.SetActive(false);
    }

    void Update()
    {
        // 깃발과 자동차 사이의 거리를 구합니다.
        float length = this.flag.transform.position.x - this.car.transform.position.x;

        // UI의 텍스트를 수정합니다.
        if (length >= 0)
        {
            time = time + Time.deltaTime;
            this.distance.GetComponent<TextMeshProUGUI>().text = "Distance : " + length.ToString("F2") + "m";
            this.timer.GetComponent<TextMeshProUGUI>().text = "Timer : " + time.ToString("F2") + "s";
        }
        else
        {
            this.distance.GetComponent<TextMeshProUGUI>().text = "Game Over!!";

            if (!isFinishSend)
            {
                ClientManager.Instance.SendFinishTime(time);
                isFinishSend = true;
                car.GetComponent<CarControllor>().isGameOver = true;
            }

            if(!string.IsNullOrEmpty(ClientManager.Instance.GetRank()))
            {
                this.rankPanel.SetActive(true);

                this.rankText.GetComponent<TextMeshProUGUI>().text = ClientManager.Instance.GetRank();
            }
        }
    }
}
