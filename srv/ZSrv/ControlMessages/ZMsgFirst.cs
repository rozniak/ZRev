using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv.ControlMessages
{
    internal class ZMsgFirst
    {
        public const uint DataSize = 0x28;

        public ZMsgHeader Header;

        public ZMsgFirst()
        {
            Header = new ZMsgHeader();
        }

        public byte[] GetBytes()
        {
            byte[] buf = new byte[DataSize];

            Header.FillBuffer(buf);

            return buf;
        }
    }
}
