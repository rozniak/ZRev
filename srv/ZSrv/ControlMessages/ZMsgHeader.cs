using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv.ControlMessages
{
    internal class ZMsgHeader
    {
        public const uint DataSize = 0x14;
        public const uint Signature = 0x4c694e6b; // 'LiNk'

        public uint Magic;
        public uint TotalSize;
        public ushort Sequence;
        public ushort ControlMsgSize;
        public uint Unknown0xc;
        public uint IdOrKeyOrChecksum;

        public ZMsgHeader() { }

        public ZMsgHeader(byte[] buf)
        {
            Magic = BitConverter.ToUInt32(buf, 0);
            TotalSize = BitConverter.ToUInt32(buf, 4);
            Sequence = BitConverter.ToUInt16(buf, 8);
            ControlMsgSize = BitConverter.ToUInt16(buf, 10);
            Unknown0xc = BitConverter.ToUInt32(buf, 12);
            IdOrKeyOrChecksum = BitConverter.ToUInt32(buf, 16);
        }

        public void FillBuffer(byte[] buf)
        {
            Array.Copy(BitConverter.GetBytes(Magic), 0, buf, 0, 4);
            Array.Copy(BitConverter.GetBytes(TotalSize), 0, buf, 4, 4);
            Array.Copy(BitConverter.GetBytes(Sequence), 0, buf, 8, 2);
            Array.Copy(BitConverter.GetBytes(ControlMsgSize), 0, buf, 10, 2);
            Array.Copy(BitConverter.GetBytes(Unknown0xc), 0, buf, 12, 2);
            Array.Copy(BitConverter.GetBytes(IdOrKeyOrChecksum), 0, buf, 16, 4);
        }
    }
}
