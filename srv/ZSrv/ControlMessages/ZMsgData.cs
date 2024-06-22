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

        public ZMsgData() { }

        public ZMsgData(ZMsgHeader header, byte[] buf)
        {
            Header = header;
            DataHeader = new ZMsgDataHeader(buf);

            Payload = new byte[DataHeader.PayloadSize];

            Array.Copy(buf, ZMsgDataHeader.DataSize, Payload, 0, DataHeader.PayloadSize);
        }
    }
}
