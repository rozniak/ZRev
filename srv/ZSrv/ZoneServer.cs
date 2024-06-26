using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ZSrv
{
    internal sealed class ZoneServer
    {
        public void Listen()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 28805);
            ZoneProtocol prot = new ZoneProtocol();

            listener.Start();

            Console.WriteLine("Listening on {0}", 28805);

            while (true)
            {
                if (listener.Pending())
                {
                    TcpClient client = listener.AcceptTcpClient();

                    Console.WriteLine("Got a client");

                    try
                    {
                        using (NetworkStream ns = client.GetStream())
                        {
                            while (true)
                            {
                                // Attempt to read
                                //
                                byte[] buf = new byte[prot.NextBytesNeeded];
                                int read = 0;
                                bool doneReading = false;

                                while (!doneReading)
                                {
                                    read += ns.Read(buf, read, ((int)prot.NextBytesNeeded - read));

                                    // Check if we need any more data
                                    //
                                    doneReading = read == prot.NextBytesNeeded;
                                }

                                prot.HandlePacket(ns, buf);
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Connection ended: {0}", ex.Message);
                    }
                }
            }
        }
    }
}
