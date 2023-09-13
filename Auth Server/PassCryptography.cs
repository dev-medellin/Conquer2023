namespace AccServer
{
    using System;
    using System.IO;
    using System.Text;
    public class PasswordCrypt
    {
        private static uint[] _key = new uint[] {0xEBE854BC, 0xB04998F7, 0xFFFAA88C, 0x96E854BB,
                                        0xA9915556, 0x48E44110, 0x9F32308F, 0x27F41D3E,
                                        0xCF4F3523, 0xEAC3C6B4, 0xE9EA5E03, 0xE5974BBA,
                                        0x334D7692, 0x2C6BCF2E, 0xDC53B74,  0x995C92A6,
                                        0x7E4F6D77, 0x1EB2B79F, 0x1D348D89, 0xED641354,
                                        0x15E04A9D, 0x488DA159, 0x647817D3, 0x8CA0BC20,
                                        0x9264F7FE, 0x91E78C6C, 0x5C9A07FB, 0xABD4DCCE,
                                        0x6416F98D, 0x6642AB5B };

        private static uint LeftRotate(uint dwVar, uint dwOffset)
        {
            uint dwTemp1, dwTemp2;

            //& the dwoffset with 0x1F
            dwOffset = dwOffset & 0x1F;
            dwTemp1 = dwVar >> (int)(32 - dwOffset);
            dwTemp2 = dwVar << (int)dwOffset;
            dwTemp2 = dwTemp2 | dwTemp1;

            return dwTemp2;
        }

        private static uint RightRotate(uint dwVar, uint dwOffset)
        {
            uint dwTemp1, dwTemp2;

            dwOffset = dwOffset & 0x1F;
            dwTemp1 = dwVar << (int)(32 - dwOffset);
            dwTemp2 = dwVar >> (int)dwOffset;
            dwTemp2 = dwTemp2 | dwTemp1;

            return dwTemp2;
        }

        public static byte[] Encrypt(string password)
        {
            byte[] result = new byte[16];
            Encoding.ASCII.GetBytes(password).CopyTo(result, 0);
            BinaryReader reader = new BinaryReader(new MemoryStream(result, false));
            uint[] passInts = new uint[4];
            for (uint i = 0; i < 4; i++)
                passInts[i] = (uint)reader.ReadInt32();


            uint temp1, temp2;
            for (int i = 1; i >= 0; i--)
            {
                temp1 = _key[5] + passInts[(i * 2) + 1];
                temp2 = _key[4] + passInts[i * 2];
                for (int j = 0; j < 12; j++)
                {
                    temp2 = LeftRotate(temp1 ^ temp2, temp1) + _key[j * 2 + 6];
                    temp1 = LeftRotate(temp1 ^ temp2, temp2) + _key[j * 2 + 7];

                }
                passInts[i * 2] = temp2;
                passInts[i * 2 + 1] = temp1;

            }
            BinaryWriter writer = new BinaryWriter(new MemoryStream(result, true));
            for (uint i = 0; i < 4; i++)
                writer.Write((int)passInts[i]);
            return result;
        }

        public static string Decrypt(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes, false));
            uint[] passInts = new uint[4];
            for (uint i = 0; i < 4; i++)
                passInts[i] = (uint)reader.ReadInt32();


            uint temp1, temp2;
            for (int i = 1; i >= 0; i--)
            {
                temp1 = passInts[(i * 2) + 1];
                temp2 = passInts[i * 2];
                for (int j = 11; j >= 0; j--)
                {
                    temp1 = RightRotate(temp1 - _key[j * 2 + 7], temp2) ^ temp2;
                    temp2 = RightRotate(temp2 - _key[j * 2 + 6], temp1) ^ temp1;

                }
                passInts[i * 2 + 1] = temp1 - _key[5];
                passInts[i * 2] = temp2 - _key[4];

            }
            BinaryWriter writer = new BinaryWriter(new MemoryStream(bytes, true));
            for (uint i = 0; i < 4; i++)
                writer.Write((int)passInts[i]);
            for (int i = 0; i < 16; i++)
                if (bytes[i] == 0)
                    return Encoding.ASCII.GetString(bytes, 0, i);
            return Encoding.ASCII.GetString(bytes);
        }
    }
    public sealed class ConquerPasswordCryptpographer
    {
        private readonly byte[] key = new byte[0x200];
        private static byte[] scanCodeToVirtualKeyMap = new byte[] {
            0, 0x1b, 0x31, 50, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0xbd, 0xbb, 8, 9,
            0x51, 0x57, 0x45, 0x52, 0x54, 0x59, 0x55, 0x49, 0x4f, 80, 0xdb, 0xdd, 13, 0x11, 0x41, 0x53,
            0x44, 70, 0x47, 0x48, 0x4a, 0x4b, 0x4c, 0xba, 0xc0, 0xdf, 0x10, 0xde, 90, 0x58, 0x43, 0x56,
            0x42, 0x4e, 0x4d, 0xbc, 190, 0xbf, 0x10, 0x6a, 0x12, 0x20, 20, 0x70, 0x71, 0x72, 0x73, 0x74,
            0x75, 0x76, 0x77, 120, 0x79, 0x90, 0x91, 0x24, 0x26, 0x21, 0x6d, 0x25, 12, 0x27, 0x6b, 0x23,
            40, 0x22, 0x2d, 0x2e, 0x2c, 0, 220, 0x7a, 0x7b, 12, 0xee, 0xf1, 0xea, 0xf9, 0xf5, 0xf3,
            0, 0, 0xfb, 0x2f, 0x7c, 0x7d, 0x7e, 0x7f, 0x80, 0x81, 130, 0x83, 0x84, 0x85, 0x86, 0xed,
            0, 0xe9, 0, 0xc1, 0, 0, 0x87, 0, 0, 0, 0, 0xeb, 9, 0, 0xc2, 0
         };
        private static byte[] virtualKeyToScanCodeMap = new byte[] {
            0, 0, 0, 70, 0, 0, 0, 0, 14, 15, 0, 0, 0x4c, 0x1c, 0, 0,
            0x2a, 0x1d, 0x38, 0, 0x3a, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
            0x39, 0x49, 0x51, 0x4f, 0x47, 0x4b, 0x48, 0x4d, 80, 0, 0, 0, 0x54, 0x52, 0x53, 0x63,
            11, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 0, 0, 0, 0, 0,
            0, 30, 0x30, 0x2e, 0x20, 0x12, 0x21, 0x22, 0x23, 0x17, 0x24, 0x25, 0x26, 50, 0x31, 0x18,
            0x19, 0x10, 0x13, 0x1f, 20, 0x16, 0x2f, 0x11, 0x2d, 0x15, 0x2c, 0x5b, 0x5c, 0x5d, 0, 0x5f,
            0x52, 0x4f, 80, 0x51, 0x4b, 0x4c, 0x4d, 0x47, 0x48, 0x49, 0x37, 0x4e, 0, 0x4a, 0x53, 0x35,
            0x3b, 60, 0x3d, 0x3e, 0x3f, 0x40, 0x41, 0x42, 0x43, 0x44, 0x57, 0x58, 100, 0x65, 0x66, 0x67,
            0x68, 0x69, 0x6a, 0x6b, 0x6c, 0x6d, 110, 0x76, 0, 0, 0, 0, 0, 0, 0, 0,
            0x45, 70, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0x2a, 0x36, 0x1d, 0x1d, 0x38, 0x38, 0x6a, 0x69, 0x67, 0x68, 0x65, 0x66, 50, 0x20, 0x2e, 0x30,
            0x19, 0x10, 0x24, 0x22, 0x6c, 0x6d, 0x6b, 0x21, 0, 0, 0x27, 13, 0x33, 12, 0x34, 0x35,
            40, 0x73, 0x7e, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x1a, 0x56, 0x1b, 0x2b, 0x29,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0x71, 0x5c, 0x7b, 0, 0x6f, 90, 0,
            0, 0x5b, 0, 0x5f, 0, 0x5e, 0, 0, 0, 0x5d, 0, 0x62, 0, 0, 0, 0
         };


        public ConquerPasswordCryptpographer(string key)
        {
            int seed = 0;
            foreach (byte num2 in Encoding.ASCII.GetBytes(key))
            {
                seed += num2;
            }
            msvcrt.msvcrt.srand(seed);
            byte[] buffer = new byte[0x10];
            for (int i = 0; i < 0x10; i++)
            {
                buffer[i] = (byte)msvcrt.msvcrt.rand();
            }
            for (int j = 1; j < 0x100; j++)
            {
                this.key[j * 2] = (byte)j;
                this.key[(j * 2) + 1] = (byte)(j ^ buffer[j & 15]);
            }
            for (int k = 1; k < 0x100; k++)
            {
                for (int m = 1 + k; m < 0x100; m++)
                {
                    if (this.key[(k * 2) + 1] < this.key[(m * 2) + 1])
                    {
                        this.key[k * 2] = (byte)(this.key[k * 2] ^ this.key[m * 2]);
                        this.key[m * 2] = (byte)(this.key[m * 2] ^ this.key[k * 2]);
                        this.key[k * 2] = (byte)(this.key[k * 2] ^ this.key[m * 2]);
                        this.key[(k * 2) + 1] = (byte)(this.key[(k * 2) + 1] ^ this.key[(m * 2) + 1]);
                        this.key[(m * 2) + 1] = (byte)(this.key[(m * 2) + 1] ^ this.key[(k * 2) + 1]);
                        this.key[(k * 2) + 1] = (byte)(this.key[(k * 2) + 1] ^ this.key[(m * 2) + 1]);
                    }
                }
            }
        }
        public byte[] Decrypt(byte[] data)
        {
            byte[] buffer = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                bool flag = false;
                if (data[i] == 0)
                {
                    return buffer;
                }
                byte index = this.key[data[i] * 2];
                if (index >= 0x80)
                {
                    index = (byte)(this.key[data[i] * 2] - 0x80);
                    flag = true;
                }
                buffer[i] = (byte)(buffer[i] + scanCodeToVirtualKeyMap[index]);
                if ((!flag && (buffer[i] >= 0x41)) && (buffer[i] <= 90))
                {
                    buffer[i] = (byte)(buffer[i] + 0x20);
                }
            }
            return buffer;
        }

        public byte[] Encrypt(byte[] data)
        {
            byte[] buffer = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                byte num2 = data[i];
                if ((data[i] >= 0x61) && (data[i] <= 0x7a))
                {
                    data[i] = (byte)(data[i] - 0x20);
                }
                byte num3 = virtualKeyToScanCodeMap[data[i]];
                if ((num2 >= 0x41) && (num2 <= 90))
                {
                    num3 = (byte)(num3 + 0x80);
                }
                for (byte j = 0; j <= 0xff; j = (byte)(j + 1))
                {
                    byte num5 = this.key[j * 2];
                    if (num5 == num3)
                    {
                        buffer[i] = j;
                        break;
                    }
                }
            }
            return buffer;
        }
    }
    public sealed class RC5
    {
        private readonly uint[] bufKey = new uint[4];
        private readonly uint[] bufSub = new uint[0x1a];

        public RC5(byte[] data)
        {
            if (data.Length != 0x10)
            {
                throw new RC5Exception("Invalid data length. Must be 16 bytes");
            }
            uint index = 0;
            uint num2 = 0;
            uint num3 = 0;
            uint num4 = 0;
            for (int i = 0; i < 4; i++)
            {
                this.bufKey[i] = (uint)(((data[i * 4] + (data[(i * 4) + 1] << 8)) + (data[(i * 4) + 2] << 0x10)) + (data[(i * 4) + 3] << 0x18));
            }
            this.bufSub[0] = 0xb7e15163;
            for (int j = 1; j < 0x1a; j++)
            {
                this.bufSub[j] = this.bufSub[j - 1] - 0x61c88647;
            }
            for (int k = 1; k <= 0x4e; k++)
            {
                this.bufSub[index] = LeftRotate((this.bufSub[index] + num3) + num4, 3);
                num3 = this.bufSub[index];
                index = (index + 1) % 0x1a;
                this.bufKey[num2] = LeftRotate((this.bufKey[num2] + num3) + num4, (int)(num3 + num4));
                num4 = this.bufKey[num2];
                num2 = (num2 + 1) % 4;
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            if ((data.Length % 8) != 0)
            {
                throw new RC5Exception("Invalid password length. Must be multiple of 8");
            }
            int num = (data.Length / 8) * 8;
            if (num <= 0)
            {
                throw new RC5Exception("Invalid password length. Must be greater than 0 bytes.");
            }
            uint[] numArray = new uint[data.Length / 4];
            for (int i = 0; i < (data.Length / 4); i++)
            {
                numArray[i] = (uint)(((data[i * 4] + (data[(i * 4) + 1] << 8)) + (data[(i * 4) + 2] << 0x10)) + (data[(i * 4) + 3] << 0x18));
            }
            for (int j = 0; j < (num / 8); j++)
            {
                uint num4 = numArray[2 * j];
                uint num5 = numArray[(2 * j) + 1];
                for (int m = 12; m >= 1; m--)
                {
                    num5 = RightRotate(num5 - this.bufSub[(2 * m) + 1], (int)num4) ^ num4;
                    num4 = RightRotate(num4 - this.bufSub[2 * m], (int)num5) ^ num5;
                }
                uint num7 = num5 - this.bufSub[1];
                uint num8 = num4 - this.bufSub[0];
                numArray[2 * j] = num8;
                numArray[(2 * j) + 1] = num7;
            }
            byte[] buffer = new byte[numArray.Length * 4];
            for (int k = 0; k < numArray.Length; k++)
            {
                buffer[k * 4] = (byte)numArray[k];
                buffer[(k * 4) + 1] = (byte)(numArray[k] >> 8);
                buffer[(k * 4) + 2] = (byte)(numArray[k] >> 0x10);
                buffer[(k * 4) + 3] = (byte)(numArray[k] >> 0x18);
            }
            return buffer;
        }

        public byte[] Encrypt(byte[] data)
        {
            if ((data.Length % 8) != 0)
            {
                throw new RC5Exception("Invalid password length. Must be multiple of 8");
            }
            int num = (data.Length / 8) * 8;
            if (num <= 0)
            {
                throw new RC5Exception("Invalid password length. Must be greater than 0 bytes.");
            }
            uint[] numArray = new uint[data.Length / 4];
            for (int i = 0; i < (data.Length / 4); i++)
            {
                numArray[i] = (uint)(((data[i * 4] + (data[(i * 4) + 1] << 8)) + (data[(i * 4) + 2] << 0x10)) + (data[(i * 4) + 3] << 0x18));
            }
            for (int j = 0; j < (num / 8); j++)
            {
                uint num4 = numArray[j * 2];
                uint num5 = numArray[(j * 2) + 1];
                uint num6 = num4 + this.bufSub[0];
                uint num7 = num5 + this.bufSub[1];
                for (int m = 1; m <= 12; m++)
                {
                    num6 = LeftRotate(num6 ^ num7, (int)num7) + this.bufSub[m * 2];
                    num7 = LeftRotate(num7 ^ num6, (int)num6) + this.bufSub[(m * 2) + 1];
                }
                numArray[j * 2] = num6;
                numArray[(j * 2) + 1] = num7;
            }
            byte[] buffer = new byte[numArray.Length * 4];
            for (int k = 0; k < numArray.Length; k++)
            {
                buffer[k * 4] = (byte)numArray[k];
                buffer[(k * 4) + 1] = (byte)(numArray[k] >> 8);
                buffer[(k * 4) + 2] = (byte)(numArray[k] >> 0x10);
                buffer[(k * 4) + 3] = (byte)(numArray[k] >> 0x18);
            }
            return buffer;
        }

        private static uint LeftRotate(uint value, int shiftAmount)
        {
            return ((value << shiftAmount) | (value >> (0x20 - (shiftAmount & 0x1f))));
        }

        private static uint RightRotate(uint value, int shiftAmount)
        {
            return ((value >> shiftAmount) | (value << (0x20 - (shiftAmount & 0x1f))));
        }
    }
    public sealed class RC5Exception : Exception
    {
        public RC5Exception(string message) : base(message)
        {
        }
    }
}

namespace msvcrt
{
    using System;

    public class msvcrt
    {
        private static int _seed = 0;

        public static short rand()
        {
            _seed *= 0x343fd;
            _seed += 0x269ec3;
            return (short)((_seed >> 0x10) & 0x7fff);
        }

        public static void srand(int seed)
        {
            _seed = seed;
        }
    }
}