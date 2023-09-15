using System;
using AccServer.Network;
using AccServer.Database;
using System.Windows.Forms;
using AccServer.Network.Sockets;
using AccServer.Network.AuthPackets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AccServer
{
    public unsafe class Program
    {
        public static Counter EntityUID;
        public static FastRandom Random = new FastRandom();
        public static ServerSocket AuthServer;
        public static World World;
        public static ushort Port = 9959;//9958
        public static Time32 Login;
        private static object SyncLogin;
        private static System.Collections.Concurrent.ConcurrentDictionary<uint, int> LoginProtection;
        private const int TimeLimit = 10000;
         private static void WorkConsole()
        {
            while (true)
            {
                try
                {
                        CommandsAI(Console.ReadLine());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        static void Main(string[] args)
        {
            Console.Title = "Altice Conquer- Account Server";
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n\tProject Altice: Conquer Online Private Server");
            Console.WriteLine("\tDeveloped by Arthur Herbieto under (Kyaly Development Solutions)");
            Console.WriteLine("\tSeptember 15, 2023 - All Rights Reserved.\n");

            // Output the description of the project and server:
            Console.WriteLine("The account server is designed to accept login data from the client and to\n"
                + "verify that the username and password combination inputted is correct with the\n"
                + "database. If the combination is correct, the client will be transferred to the\n"
                + "message server of their choice. Please wait for the database to be initialized.\n");
            Database.DataHolder.CreateConnection();
            World = new World();
            World.Init();
            EntityUID = new Counter(0);
            LoginProtection = new System.Collections.Concurrent.ConcurrentDictionary<uint, int>();
            SyncLogin = new object();
            Database.Server.Load();
            Console.WriteLine("\nStarting the server...");
            Network.Cryptography.AuthCryptography.PrepareAuthCryptography();
            AuthServer = new ServerSocket();
            AuthServer.OnClientConnect += AuthServer_OnClientConnect;
            AuthServer.OnClientReceive += AuthServer_OnClientReceive;
            AuthServer.OnClientDisconnect += AuthServer_OnClientDisconnect;
            AuthServer.Enable(Port, "0.0.0.0");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Connection Port " + Port);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("The server is ready for incoming connections!\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("");
            WorkConsole();
            CommandsAI(Console.ReadLine());
        }
        public static void CommandsAI(string command)
        {
            if (command == null) return;
            string[] data = command.Split(' ');
            switch (data[0])
            {
                case "@memo":
                    {
                        var proc = System.Diagnostics.Process.GetCurrentProcess();
                        Console.WriteLine("Thread count: " + proc.Threads.Count);
                        Console.WriteLine("Memory set(MB): " + ((double)((double)proc.WorkingSet64 / 1024)) / 1024);
                        proc.Close();
                        break;
                    }
                case "@a":
                    {
                        Console.Clear();
                        break;
                    }
                case "@restart":
                    {
                        AuthServer.Disable();
                        Application.Restart();
                        Environment.Exit(0);
                        break;
                    }
            }
        }
        public static List<string> HwidBanned = new List<string>();
        private static void AuthServer_OnClientReceive(byte[] buffer, int length, ClientWrapper arg3)
        {
            var player = arg3.Connector as Client.AuthClient;
            player.Cryptographer.Decrypt(buffer, length);
            player.Queue.Enqueue(buffer, length);
            while (player.Queue.CanDequeue())
            {
                byte[] packet = player.Queue.Dequeue();
                ushort len = BitConverter.ToUInt16(packet, 0);
                ushort id = BitConverter.ToUInt16(packet, 2);
                if (len == 276)
                {
                    player.Info = new Authentication();
                    player.Info.Seed = player.PasswordSeed;

                    player.Info.Deserialize(packet);
                    //if(player.Info.Hwid == "")
                    //{
                    //    player.Disconnect();
                    //    return;
                    //}
                    player.Account = new AccountTable(player.Info.Username);
                    player.Account.IP = arg3.IP;
                    //Database.ServerInfo Server = null;
                    Forward Fw = new Forward();
                    //if (Database.Server.Servers.TryGetValue(player.Info.Server, out Server))
                    {
                        if (!player.Account.exists)
                        {
                            Fw.Type = Forward.ForwardType.WrongAccount;
                        }

                        if (player.Account.Password == player.Info.Password && player.Account.exists)
                        {
                            Fw.Type = Forward.ForwardType.Ready;
                            if (player.Account.EntityID == 0)
                            {
                                using (MySqlCommand cmd = new MySqlCommand(MySqlCommandType.SELECT))
                                {
                                    cmd.Select("configuration");
                                    using (MySqlReader r = new MySqlReader(cmd))
                                    {
                                        if (r.Read())
                                        {
                                            EntityUID = new Counter(r.ReadUInt32("EntityID"));
                                            player.Account.EntityID = EntityUID.Next;
                                            using (MySqlCommand cmd2 = new MySqlCommand(MySqlCommandType.UPDATE).Update("configuration")
                                            .Set("EntityID", player.Account.EntityID)) cmd2.Execute();
                                            player.Account.Hwid = player.Info.Hwid;
                                            player.Account.Save();
                                        }
                                    }
                                }
                            }
                        }
                        if (Fw.Type != Forward.ForwardType.Ready)
                        {
                            Fw.Type = Forward.ForwardType.InvalidInfo;
                        }
                        if (player.Account.Banned || HwidBanned.Contains(player.Info.Hwid))
                        {
                            Fw.Type = Forward.ForwardType.Banned;
                        }
                        lock (SyncLogin)
                        {
                            if (Fw.Type == Forward.ForwardType.Ready)
                            {
                                player.Account.SaveIP();
                                TransferCipher transferCipher = new TransferCipher("EypKhLvYJ3zdLCTyz9Ak8RAgM78tY5F32b7CUXDuLDJDFBH8H67BWy9QThmaN5VS", "MyqVgBf3ytALHWLXbJxSUX4uFEu3Xmz2UAY9sTTm8AScB7Kk2uwqDSnuNJske4BJ", "26.34.47.52");
                                uint[] encrypted = transferCipher.Encrypt(new uint[] { player.Account.EntityID, (uint)player.Account.State });
                                Fw.Identifier = encrypted[0];
                                Fw.State = (uint)encrypted[1];
                                Fw.IP = "26.34.47.52";
                                Fw.Port = 5816;
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine("{0} has been Login to server {1}! IP:[{2}].", player.Info.Username, player.Info.Server, player.IP);
                            }
                            player.Send(Fw);
                        }
                    }
                }
            }
           
        }
        private static void AuthServer_OnClientDisconnect(ClientWrapper obj)
        {
            obj.Disconnect();
        }
        private static void AuthServer_OnClientConnect(ClientWrapper obj)
        {
            Client.AuthClient authState;
            obj.Connector = (authState = new Client.AuthClient(obj));
            authState.Cryptographer = new Network.Cryptography.AuthCryptography();
            Network.AuthPackets.PasswordCryptographySeed pcs = new PasswordCryptographySeed();
            pcs.Seed = Program.Random.Next();
            authState.PasswordSeed = pcs.Seed;
            authState.Send(pcs);
        }
    }
}