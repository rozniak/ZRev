using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv.ControlMessages
{
    internal sealed class ZMsgData
    {
        public ZMsgHeader Header;
        public ZMsgDataHeader DataHeader;
        public byte[] Payload;

        public uint DataSize
        {
            get { return ZMsgHeader.DataSize + ZMsgDataHeader.DataSize + (uint) Payload.Length + 4; }
        }

        public ZMsgData()
        {
            Header = new ZMsgHeader();
            DataHeader = new ZMsgDataHeader();
        }

        public ZMsgData(ZMsgHeader header, byte[] buf)
        {
            Header = header;
            DataHeader = new ZMsgDataHeader(buf);

            Payload = new byte[DataHeader.PayloadSize];

            Array.Copy(buf, ZMsgDataHeader.DataSize, Payload, 0, DataHeader.PayloadSize);
        }

        public void FillBuffer(byte[] buf)
        {
            DataHeader.FillBuffer(buf);

            Array.Copy(Payload, 0, buf, ZMsgHeader.DataSize + ZMsgDataHeader.DataSize, Payload.Length);

            // Need to work out the checksum before we can write the header
            //
            uint checksum = ZSecurity.GenerateChecksum(1, buf,  ZMsgDataHeader.DataSize + (uint) Payload.Length, ZMsgHeader.DataSize);

            Header.IdOrKeyOrChecksum = checksum;
            Header.FillBuffer(buf);

            // Stupid extra DWORD just for saying it's 'encrypted' when it isn't
            //
            Array.Copy(BitConverter.GetBytes(1), 0, buf, buf.Length - 4, 4);
        }
    }
}
