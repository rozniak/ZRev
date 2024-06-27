using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZSrv
{
    internal sealed class ZoneServer
    {
        public void Listen()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 28805);

            listener.Start();

            Console.WriteLine("Listening on {0}", 28805);

            while (true)
            {
                if (listener.Pending())
                {
                    TcpClient client = listener.AcceptTcpClient();

                    ThreadPool.QueueUserWorkItem(ClientProc, client);
                }
            }
        }

        private static void ClientProc(object client)
        {
            var tcpClient = (TcpClient)client;

            EndPoint endPoint = tcpClient.Client.RemoteEndPoint;
            ZoneProtocolClient prot = new ZoneProtocolClient(endPoint);

            try
            {
                ZLogging.LogServerMessage(
                    string.Format(
                        "New connection at {0}",
                        endPoint
                    )
                );

                using (NetworkStream ns = tcpClient.GetStream())
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
                ZLogging.LogServerMessage(
                    string.Format(
                        "Connection ended for {0}, reason: {1}",
                        endPoint,
                        ex.Message
                    )
                );
            }
        }
    }
}
