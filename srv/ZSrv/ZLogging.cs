using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv
{
    internal static class ZLogging
    {
        public static void LogClientMessage(EndPoint endPoint, string message)
        {
            Console.WriteLine("[{0}] {1}", endPoint, message);
        }

        public static void LogServerMessage(string message)
        {
            Console.WriteLine("[SERVER] {0}", message);
        }
    }
}
