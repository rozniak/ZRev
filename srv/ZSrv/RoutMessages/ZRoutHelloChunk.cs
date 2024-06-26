using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv.RoutMessages
{
    internal sealed class ZRoutHelloChunk : ZRoutMsgChunkBase
    {
        public override ushort Type { get { return 1; } }

        public ZRoutHelloChunk() { }

        public override byte[] GetBytes()
        {
            byte[] buf = new byte[4];

            Array.Copy(BitConverter.GetBytes(Type), 0, buf, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort)buf.Length), 0, buf, 2, 2);

            return buf;
        }
    }
}
