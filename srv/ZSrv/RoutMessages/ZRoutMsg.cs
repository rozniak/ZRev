using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZSrv.ControlMessages;

namespace ZSrv.RoutMessages
{
    internal sealed class ZRoutMsg
    {
        public List<ZRoutMsgChunkBase> Chunks;

        public ZRoutMsg()
        {
            Chunks = new List<ZRoutMsgChunkBase>();
        }

        public ZRoutMsg(ZMsgData msgData)
            : this()
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytes()
        {
            var buf = new List<byte>();

            foreach (ZRoutMsgChunkBase chunk in Chunks)
            {
                buf.AddRange(chunk.GetBytes());
            }

            return buf.ToArray();
        }
    }
}
