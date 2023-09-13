using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace AutoPatch_Server
{
    public class SocketWrapper
    {
        MainSocket MainServer;
        Socket Socket;
        byte[] Buffer;
        public string IP
        {
            get { return (Socket.RemoteEndPoint as IPEndPoint).Address.ToString(); }
        }
        public SocketWrapper(Socket Socket, MainSocket MainServer, int BufferLength = 2048)
        {
            this.Socket = Socket;
            this.MainServer = MainServer;
            this.Buffer = new byte[BufferLength];
            Program.MyConnectedClients.Add(this);
        }
        public void TryRec()
        {
            try
            {
                Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(BeginRec), Socket);
            }
            catch
            {
                Socket.Disconnect(false);
            }
        }
        private void BeginRec(IAsyncResult ar)
        {
            try
            {
                int recLength = Socket.EndReceive(ar);
                if (recLength != 0)
                {
                    byte[] buffer = new byte[recLength];
                    Array.Copy(Buffer, buffer, recLength);
                    HandlePacket(buffer, this);
                    TryRec();
                }
            }
            catch
            {
                Disconnect();
            }
        }
        private void Disconnect()
        {
            try
            {
                Socket.Disconnect(false);
                Program.MyConnectedClients.Remove(this);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Error on disconnection] --> " + e.ToString());
            }
        }
        private void Send(byte[] p)
        {
            try
            {
                Socket.Send(p);
            }

            catch (Exception e)
            {
                Console.WriteLine($"[Error on sending] --> {e.ToString()}");
            }
        }
        private void HandlePacket(byte[] buffer, SocketWrapper client)
        {
            try
            {
                string res = Encoding.Default.GetString(buffer);
                int version = int.Parse(res);
                string patch = GetLink(++version);
                if (patch != "READY")
                    Console.WriteLine($"[{client.IP}] v:{version} link: {patch}");
                else
                    Console.WriteLine($"Completed Login on {client.IP}");
                Send(Encoding.Default.GetBytes(patch));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error on handling packet --> {e.ToString()}");
            }
        }
        private string GetLink(int v)
        {
            try
            {
                if (Program.PatchesPath.ContainsKey(v.ToString()))
                    return $"UPDATE {Program.updatepath} patches/{Program.PatchesPath[v.ToString()]}";
                else
                    return "READY";
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error on link --> {e.ToString()}");
                return "READY";
            }
        }
    }
}
