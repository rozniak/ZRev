using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv.RoutMessages
{
    internal sealed class ZRoutAbilitiesChunk : ZRoutMsgChunkBase
    {
        public override ushort Type { get { return 7; } }

        public ushort ChatAbility;
        public ushort StatsAbility;

        public ZRoutAbilitiesChunk()
        {
            // For now... just set these to random values, they must be
            // between 1-3
            //
            var rand = new Random();

            ChatAbility = (ushort) rand.Next(1, 4);
            StatsAbility = (ushort) rand.Next(1, 4);
        }

        public override byte[] GetBytes()
        {
            byte[] buf = new byte[8];

            Array.Copy(BitConverter.GetBytes(Type), 0, buf, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort) buf.Length), 0, buf, 2, 2);
            Array.Copy(BitConverter.GetBytes(ChatAbility), 0, buf, 4, 2);
            Array.Copy(BitConverter.GetBytes(StatsAbility), 0, buf, 6, 2);

            return buf;
        }
    }
}
