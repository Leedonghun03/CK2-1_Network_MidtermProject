using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance;

    private TcpClient m_Client;
    private NetworkStream stream;
    private Thread recvThread;

    private const int SERVER_PORT = 9000;
    private const int BUF_SIZE = 2048;
    private string SERVER_IP = "127.0.0.1";

    private int myClientID = -1;
    private string RankingData = "";
    
    [SerializeField]
    private OtherPlayerCar otherPlayerCar;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        try
        {
            m_Client = new TcpClient(SERVER_IP, SERVER_PORT);
            stream = m_Client.GetStream();

            recvThread = new Thread(GetMessage);
            recvThread.IsBackground = true;
            recvThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ���� ���� : {e.Message}");
        }
    }

    void GetMessage()
    {
        byte[] buf = new byte[BUF_SIZE];
        int size;
        string output;

        while (true)
        {
            try
            {
                size = stream.Read(buf, 0, buf.Length);
                output = Encoding.Default.GetString(buf, 0, size);
                Debug.Log($"������ ���� ���� : {output}");

                if (myClientID == -1 && int.TryParse(output, out int ID))
                {
                    myClientID = ID;
                    Debug.Log($"�� Ŭ���̾�Ʈ ID: {myClientID}");
                    continue;
                }

                // ���� ������ ex) Rank:[1. Player0 = 4.37s][2. Player0 = 4.23s][3. Player0 = 3.36s] �̷������� ����
                if (output.StartsWith("Rank:"))
                {
                    string rankData = output.Substring("Rank:".Length);
                    SetMessageRank(rankData);
                }

                // ���� ������ ex) Position:0,9.90241,-2.40241
                if (output.StartsWith("Position:"))
                {
                    string positionData = output.Substring("Position:".Length);
                    GetPositionMessage(positionData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"���� �� ���� �߻� : {e.Message}");
                break;
            }
        }
    }

    void GetPositionMessage(string Data)
    {
        // ���� ������ ex) Position:0,9.90241,-2.40241
        string[] parts = Data.Split(",");

        if (parts.Length == 3 && int.TryParse(parts[0], out int senderID))
        {
            if (senderID != myClientID)
            {
                float lengthX = float.Parse(parts[1]);
                float posX = float.Parse(parts[2]);

                if(otherPlayerCar != null)
                {
                    otherPlayerCar.SetTargetPosition(posX);
                }
            }
        }
    }

    void SetMessageRank(string Data)
    {
        // ���� ������ ex) Rank:[1. Player0 = 4.37s]\n[2. Player0 = 4.23s]\n[3. Player0 = 3.36s]\n �̷������� ����
        RankingData = Data;
    }

    public string GetRank()
    {
        return RankingData;
    }

    public void SendMyPosition(float length, Vector2 pos)
    {
        if (stream == null || !m_Client.Connected)
        {
            Debug.LogWarning("���� ��Ʈ���� ��ȿ���� �ʽ��ϴ�.");
            return;
        }

        string data = length.ToString() + "," + pos.x;
        byte[] sendBuf = Encoding.Default.GetBytes(data);
        int size = sendBuf.Length;
        if (size > BUF_SIZE)
        {
            size = BUF_SIZE;
        }

        try
        {
            stream.Write(sendBuf, 0, size);
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ���� : {e.Message}");
        }
    }

    public void SendFinishTime(float Time)
    {
        if (stream == null || !m_Client.Connected)
        {
            return;
        }

        string data = $"FINISH:{Time}";
        byte[] sendBuf = Encoding.Default.GetBytes(data);
        int size = sendBuf.Length;
        if(size > BUF_SIZE)
        {
            size = BUF_SIZE;
        }

        try
        {
            stream.Write(sendBuf, 0, size);
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ���� : {e.Message}");
        }
    }

    private void SafeClose()
    {
        if (stream != null)
        {
            stream.Close();
            stream = null;
        }

        if (m_Client != null)
        {
            m_Client.Close();
            m_Client = null;
        }
    }

    private void OnApplicationQuit()
    {
        SafeClose();
    }
}
