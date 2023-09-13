﻿using System;
using System.Text;

namespace AccServer.Network.AuthPackets
{
    public unsafe class Forward : Interfaces.IPacket
    {
        public enum ForwardType : byte
        {
            Ready = 2,
            InvalidInfo = 1,
            WrongAccount = 57,
            ServersNotConfigured = 59,
            InvalidAuthenticationProtocol = 73,
            Banned = 25
        }
        private byte[] Buffer;
        public Forward()
        {
            Buffer = new byte[52];
            Network.Writer.WriteUInt16(52, 0, Buffer);
            Network.Writer.WriteUInt16(1055, 2, Buffer);
        }
        public uint Identifier
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { Network.Writer.WriteUInt32(value, 4, Buffer); }
        }
        public uint State
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { Network.Writer.WriteUInt32(value, 8, Buffer); }
        }
        public ForwardType Type
        {
            get { return (ForwardType) (byte) BitConverter.ToUInt32(Buffer, 8); }
            set { Network.Writer.WriteUInt32((byte) value, 8, Buffer); }
        }
        public string IP
        {
            get { return Encoding.Default.GetString(Buffer, 20, 16); }
            set { Network.Writer.WriteString(value, 20, Buffer); }
        }
        public ushort Port
        {
            get { return BitConverter.ToUInt16(Buffer, 12); }
            set { Network.Writer.WriteUInt16(value, 12, Buffer); }
        }
        public void Deserialize(byte[] buffer)
        {
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
    }
}