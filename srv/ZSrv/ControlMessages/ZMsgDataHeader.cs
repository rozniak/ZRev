﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv.ControlMessages
{
    internal sealed class ZMsgDataHeader
    {
        public const uint DataSize = 0x10;

        public uint Identifier;
        public uint Unknown_0x4;
        public uint SequenceNumber;
        public uint PayloadSize;

        public ZMsgDataHeader() { }

        public ZMsgDataHeader(byte[] buf)
        {
            Identifier = BitConverter.ToUInt32(buf, 0);
            Unknown_0x4 = BitConverter.ToUInt32(buf, 4);
            SequenceNumber = BitConverter.ToUInt32(buf, 8);
            PayloadSize = BitConverter.ToUInt32(buf, 12);
        }

        public void FillBuffer(byte[] buf)
        {
            Array.Copy(BitConverter.GetBytes(Identifier), 0, buf, 0, 4);
            Array.Copy(BitConverter.GetBytes(Unknown_0x4), 0, buf, 4, 4);
            Array.Copy(BitConverter.GetBytes(SequenceNumber), 0, buf, 8, 4);
            Array.Copy(BitConverter.GetBytes(PayloadSize), 0, buf, 12, 4);
        }
    }
}
