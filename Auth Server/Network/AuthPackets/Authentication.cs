using System;
using System.IO;
using System.Text;


namespace AccServer.Network.AuthPackets
{
    public unsafe class Authentication : Interfaces.IPacket
    {
        public string Username;
        public string Password;
        public string Server;
        public string Hwid;
        public string Mac;

        public Authentication()
        {
        }
        private byte[] _array;
        internal int Seed;

        public string ReadString(int length, int offset)
        {
            // Read the value:
            fixed (byte* ptr = _array)
                return new string((sbyte*)ptr, offset, length, Encoding.GetEncoding(1252)).TrimEnd('\0');
        }
        public void Deserialize(byte[] buffer)
        {
            if (buffer.Length == 276)
            {
                ushort PacketID = BitConverter.ToUInt16(buffer, 2);
                string USER = Encoding.Default.GetString(buffer, 4, 16);
                string PASS = Encoding.Default.GetString(buffer, 132, 16);
                string SERVER = Encoding.Default.GetString(buffer, 260, 16);
                string HWID = Encoding.Default.GetString(buffer, 32, 40);
                if (PacketID == 1986 || PacketID == 1060)//1086
                {
                    Username = USER.Replace("\0", "");
                    Password = PASS.Replace("\0", "");
                    Server = SERVER.Replace("\0", "");
                    Hwid = HWID.Replace("\0", "");
                }
            }
            msvcrt.msvcrt.srand(Seed);

            byte[] encpw = new byte[16];
            var rc5Key = new byte[0x10];
            for (int i = 0; i < 0x10; i++)
                rc5Key[i] = (byte)msvcrt.msvcrt.rand();
            Buffer.BlockCopy(buffer, 132, encpw, 0, 16);
            var password = System.Text.Encoding.ASCII.GetString(
                                 (new ConquerPasswordCryptpographer(Username)).Decrypt(
                                     (new RC5(rc5Key)).Decrypt(encpw)));


            Password = password.Split('\0')[0];
            string NoNumPadNumbers = "";
            foreach (char c in password)
            {
                switch (c.ToString())
                {
                    case "-": NoNumPadNumbers += "0"; break;
                    case "#": NoNumPadNumbers += "1"; break;
                    case "(": NoNumPadNumbers += "2"; break;
                    case "\"": NoNumPadNumbers += "3"; break;
                    case "%": NoNumPadNumbers += "4"; break;
                    case "\f": NoNumPadNumbers += "5"; break;
                    case "'": NoNumPadNumbers += "6"; break;
                    case "$": NoNumPadNumbers += "7"; break;
                    case "&": NoNumPadNumbers += "8"; break;
                    case "!": NoNumPadNumbers += "9"; break;
                    default: NoNumPadNumbers += c; break;
                }
            }
            Password = NoNumPadNumbers.Replace("\0", "");
        }
        public byte[] ToArray()
        {
            throw new NotImplementedException();
        }


    }
}