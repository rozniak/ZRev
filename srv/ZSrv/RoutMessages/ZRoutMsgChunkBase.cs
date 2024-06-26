using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv.RoutMessages
{
    internal abstract class ZRoutMsgChunkBase
    {
        public abstract ushort Type { get; }

        public abstract byte[] GetBytes();
    }
}
