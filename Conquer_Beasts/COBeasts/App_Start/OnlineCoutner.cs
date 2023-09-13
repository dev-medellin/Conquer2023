using MySql.Data.MySqlClient;
using PhoenixConquer.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;

namespace PhoenixConquer.App_Start
{
    public class OnlineCounter
    {
        public static string Online = "0";
        public static bool SvrOnline = false;
        public static DateTime LastRefresh;
        public static void GetOnline()
        {
            if (DateTime.Now < LastRefresh.AddMinutes(1))
                return;
            LastRefresh = DateTime.Now;
            using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    sock.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9958));
                    SvrOnline = true;
                }
                catch
                {
                    SvrOnline = false;
                }
            }

            // Designed in this stupid way to prevent multiple connections 
            // refreshs every 1 min.
            using (var conn = new MySqlConnection(UserController.connectionString()))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("select Online from cfg", conn))
                using (var rdr = cmd.ExecuteReader())
                    if (rdr.Read())
                        Online = rdr.GetInt32("Online").ToString();

            }

        }
    }
}