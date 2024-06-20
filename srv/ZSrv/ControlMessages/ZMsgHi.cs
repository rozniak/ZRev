using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv.ControlMessages
{
    internal class ZMsgHi
    {
        public const uint DataSize = 0x30;

        public ZMsgHeader Header;
        public Guid ClientGuid;

        public ZMsgHi() { }

        public ZMsgHi(byte[] buf)
        {
            byte[] guid = new byte[16];
            Array.Copy(buf, 0x20, guid, 0, 0x10);

            Header = new ZMsgHeader(buf);
            ClientGuid = new Guid(guid);
        }
    }
}
