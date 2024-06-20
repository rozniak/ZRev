using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ZoneServer server = new ZoneServer();

            Console.WriteLine("Custom Zone Server Implementation");

            server.Listen();
        }
    }
}
