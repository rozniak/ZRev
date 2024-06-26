using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv
{
    internal static class ZSecurity
    {
        public const uint ChecksumStart = 0x12344321;
        public const uint DefaultKey = 0xf8273645;

        public static void Encrypt(byte[] buf, uint len, uint key)
        {
            byte[] keyBytes = BitConverter.GetBytes(key);

            ReverseEndianness32(keyBytes);

            for (int i = 0; i < len; i++)
            {
                buf[i] = (byte)(buf[i] ^ keyBytes[i % 4]);
            }
        }

        public static uint GenerateChecksum(uint passes, byte[] buf, uint len, uint offset = 0)
        {
            byte[] checksumBytes = BitConverter.GetBytes(ChecksumStart);
            uint index = offset;
            uint passesLeft = passes;

            ReverseEndianness32(checksumBytes);

            while (passesLeft != 0)
            {
                for (uint i = index; i < len; i++)
                {
                    checksumBytes[i % 4] = (byte)(buf[i] ^ checksumBytes[i % 4]);
                }

                index += 4;
                passesLeft--;
            }

            // Report checksum
            //
            ReverseEndianness32(checksumBytes);

            return BitConverter.ToUInt32(checksumBytes, 0);
        }

        private static void ReverseEndianness32(byte[] buf)
        {
            byte tmp0 = buf[0];
            byte tmp1 = buf[1];

            buf[0] = buf[3];
            buf[1] = buf[2];
            buf[2] = tmp1;
            buf[3] = tmp0;
        }
    }
}
