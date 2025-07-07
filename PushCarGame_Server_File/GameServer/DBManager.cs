using System;

using MySql.Data.MySqlClient;

namespace GameServer
{
    class DBManager
    {
        private static string strConn = "Server = localhost; Database = ckgame; Uid = root; Pwd = Dlwmsk051216!";

        public static void InsertFinishTime(int ClientID, float FinishTime)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(strConn);
                
                conn.Open();
                string sql = $"INSERT INTO GameRankTbl VALUES (null, {ClientID}, {FinishTime});";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine($"DB 오류 : {e.Message}");
            }
        }

        public static void UpdateFinishTime(int ClientID, float FinishTime)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(strConn);

                conn.Open();
                string sql = $"UPDATE GameRankTbl SET Finish_Time = {FinishTime} WHERE Client_ID = {ClientID}";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"DB 오류 : {e.Message}");
            }
        }
        
        public static bool IsPlayerExistence(int ClientID)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(strConn);

                conn.Open();
                string sql = $"SELECT * FROM GameRankTbl WHERE Client_ID = {ClientID}";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if ((int)reader[1] == ClientID)
                    {
                        return true;
                    }
                }

                reader.Close();
                conn.Close();
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"DB 오류: {e.Message}");
                return false;
            }
        }

        public static string GetRanking()
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(strConn);
            
                conn.Open();
                string sql = "SELECT * FROM GameRankTbl ORDER BY finish_time ASC LIMIT 10;";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                string readRankString = "Rank:";
                int rank = 1;

                while(reader.Read())
                {
                    int PlayerID = (int)reader[1];
                    float RankTime = (float)reader[2];

                    readRankString += ($"[{rank}. Player{PlayerID} = {RankTime:F2}s]\n");
                    rank++;
                }

                reader.Close();
                conn.Close();
                return readRankString;
            }
            catch(Exception e)
            {
                Console.WriteLine($"DB 오류 : {e.Message}");
                return "";
            }
        }
    }
}
