using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace GameServer
{
    class TcpServer
    {
        const int SERVERPROT = 9000;
        const int BUFSIZE = 2048;

        static Dictionary<Socket, int> clientMap = new Dictionary<Socket, int>();
        static int clientIDCount = 0;
        static List<Socket> socketList = new List<Socket>();

        static void Main(string[] args)
        {
            Socket listen_Sock = null;

            try
            {
                // 소켓 생성
                listen_Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Bind()
                listen_Sock.Bind(new IPEndPoint(IPAddress.Any, SERVERPROT));
                // Listen()
                listen_Sock.Listen(Int32.MaxValue);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            // 데이터 통신에 사용할 변수
            Socket client_Sock = null;
            IPEndPoint clientAddr = null;

            while (true)
            {
                try
                {
                    // accept()
                    client_Sock = listen_Sock.Accept();
                    socketList.Add(client_Sock);

                    // 클라이언트 ID 할당
                    clientMap[client_Sock] = clientIDCount++;
                    int assigendID = clientMap[client_Sock];

                    byte[] idBuf = Encoding.Default.GetBytes(assigendID.ToString());
                    client_Sock.Send(idBuf);

                    // 접속한 클라이언트 정보 출력
                    clientAddr = (IPEndPoint)client_Sock.RemoteEndPoint;
                    Console.WriteLine($"\n[TCP SERVER] 클라이언트 접속 (IP : {clientAddr.Address}, 포트 번호 : {clientAddr.Port})");

                    // 스레드 생성
                    Thread thread = new Thread(() => ProcessClient(client_Sock, clientAddr));
                    thread.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }

            // 소켓 닫기
            listen_Sock.Close();
        }

        static void ProcessClient(Socket sock, IPEndPoint ip)
        {
            int size;
            Socket client_Sock = sock;
            IPEndPoint clientAddr = ip;
            byte[] buf = new byte[BUFSIZE];

            while (true)
            {
                try
                {
                    // 데이터 받기
                    size = client_Sock.Receive(buf, buf.Length, SocketFlags.None);
                    if (size == 0) break;

                    // 받은 데이터 출력
                    string receivedData = Encoding.Default.GetString(buf, 0, size);
                    Console.WriteLine($"[TCP {clientAddr.Address}, {clientAddr.Port}, {receivedData}]");

                    // 게임 클리어 로직
                    if (receivedData.StartsWith("FINISH:"))
                    {
                        string[] parts = receivedData.Split(':');
                        float finishTime = float.Parse(parts[1]);

                        if(DBManager.IsPlayerExistence(clientMap[sock]))
                        {
                            DBManager.UpdateFinishTime(clientMap[sock], finishTime);
                        }
                        else
                        {
                            DBManager.InsertFinishTime(clientMap[sock], finishTime);
                        }

                        string rankingData = DBManager.GetRanking();
                        buf = Encoding.Default.GetBytes(rankingData);

                        // 데이터 보내기
                        foreach (var ClientSocket in socketList)
                        {
                            try
                            {
                                ClientSocket.Send(buf, buf.Length, SocketFlags.None);
                            }
                            catch (SocketException ex)
                            {
                                Console.WriteLine($"클라이언트 전송 실패: {ex.Message}");
                                ClientSocket.Close();
                                socketList.Remove(ClientSocket); // 예외난 소켓 제거
                            }
                        }
                    }
                    else
                    {
                        // 송신할 데이터 앞에 client ID 붙이기
                        int senderId = clientMap[sock];
                        string messageToSend = $"Position:{senderId},{receivedData}";
                        buf = Encoding.Default.GetBytes(messageToSend);

                        // 데이터 보내기
                        foreach (var ClientSocket in socketList)
                        {
                            try
                            {
                                ClientSocket.Send(buf, buf.Length, SocketFlags.None);
                            }
                            catch (SocketException ex)
                            {
                                Console.WriteLine($"클라이언트 전송 실패: {ex.Message}");
                                ClientSocket.Close();
                                socketList.Remove(ClientSocket); // 예외난 소켓 제거
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }

            // 소켓 닫기
            client_Sock.Close();
            Console.WriteLine($"[TCP SERVER] : 클라이언트 종료 (IP : {clientAddr.Address}, 포트 번호 : {clientAddr.Port})");
            socketList.Remove(sock);
            sock.Close();
        }
    }
}
