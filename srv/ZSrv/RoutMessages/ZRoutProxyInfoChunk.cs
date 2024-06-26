using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv.RoutMessages
{
    internal sealed class ZRoutProxyInfoChunk : ZRoutMsgChunkBase
    {
        public override ushort Type { get { return 5; } }

        public string Game;
        public bool ServerAvailable;

        public ZRoutProxyInfoChunk(string game)
        {
            Game = game;
            ServerAvailable = true;
        }

        public override byte[] GetBytes()
        {
            byte[] buf = new byte[0x28];

            Array.Copy(BitConverter.GetBytes(Type), 0, buf, 0, 2);
            Array.Copy(BitConverter.GetBytes((ushort) buf.Length), 0, buf, 2, 2);

            // Literally don't care about 90% of the struct, except the game
            // name and flags (which we barely know anything about)
            //
            byte flags = (byte) (ServerAvailable ? 0x1 : 0);
            byte[] gameNameBytes = Encoding.ASCII.GetBytes(Game);

            Array.Copy(gameNameBytes, 0, buf, 8, gameNameBytes.Length);
            buf[0x1c] = flags;

            return buf;
        }
    }
}
