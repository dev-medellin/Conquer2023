using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AutoPatch_Server
{
    class Program
    {
        public const string updatepath = "25.32.164.214"; //95.141.33.28
        public static Dictionary<string, string> PatchesPath = new Dictionary<string, string>();
        public static List<SocketWrapper> MyConnectedClients = new List<SocketWrapper>();
        static void Main(string[] args)
        {
            using (var stream = new StreamReader(@"patch.txt"))
            {
                Console.WriteLine("Loading patch list");
                string[] line = stream.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < line.Length; i++)
                {
                    string[] u = line[i].Split(' ');
                    PatchesPath.Add(u[0], u[1]);
                    Console.WriteLine($"{u[0]} {u[1]}");
                }
            }
            int Port = 9539;
            var server = new MainSocket(Port);
            Console.WriteLine($"Socket is alive on port {Port}");
            Console.Title = $"AutoPatch Server - {Port}";
            while (true)
                Console.ReadLine();
        }
    }
}
