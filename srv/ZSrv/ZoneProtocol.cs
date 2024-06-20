using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ZSrv.ControlMessages;

namespace ZSrv
{
    internal enum ZoneProtocolReadState
    {
        Hi = 1,
        DataHeader = 2,
        Data = 3,
        FirstMsg = 4
    }

    internal sealed class ZoneProtocol
    {
        public uint NextBytesNeeded { get; private set; }

        private ZMsgHeader LastMessageHeader;
        private ZoneProtocolReadState ReadState;
        private ushort Sequence;

        public ZoneProtocol()
        {
            SetReadState(ZoneProtocolReadState.Hi);

            Sequence = 1;
        }

        public void HandlePacket(NetworkStream ns, byte[] buf)
        {
            switch (ReadState)
            {
                case ZoneProtocolReadState.Hi:
                    // Read in Hi message
                    //
                    ZSecurity.Encrypt(buf, ZMsgHi.DataSize, ZSecurity.DefaultKey);
                    ZMsgHi msgHi = new ZMsgHi(buf);
                    Console.WriteLine("Received hi message from {0}!", msgHi.ClientGuid);
                    
                    // Handle response
                    //
                    ZMsgFirst replyMsgFirst = new ZMsgFirst();

                    replyMsgFirst.Header.Magic = ZMsgHeader.Signature;
                    replyMsgFirst.Header.TotalSize = ZMsgFirst.DataSize;
                    replyMsgFirst.Header.Sequence = ++Sequence;
                    replyMsgFirst.Header.ControlMsgSize = (ushort) ZMsgFirst.DataSize;
                    replyMsgFirst.Header.Unknown0xc = 0; // TODO: Figure this out
                    replyMsgFirst.Header.IdOrKeyOrChecksum = 0; // Set the encryption key to 0

                    byte[] respBuf = replyMsgFirst.GetBytes();

                    ZSecurity.Encrypt(respBuf, ZMsgHeader.DataSize, ZSecurity.DefaultKey);
                    ns.Write(respBuf, 0, (int) ZMsgFirst.DataSize);

                    Console.WriteLine("Responded with First Message, and now we wait for ze data...");

                    SetReadState(ZoneProtocolReadState.DataHeader);

                    break;

                case ZoneProtocolReadState.DataHeader:
                    // Should no longer be 'encrypted'...
                    //
                    LastMessageHeader = new ZMsgHeader(buf);

                    Sequence = LastMessageHeader.Sequence;

                    Console.WriteLine(
                        "Data header received, expecting {0} bytes...",
                        LastMessageHeader.TotalSize - LastMessageHeader.ControlMsgSize
                    );

                    SetReadState(ZoneProtocolReadState.Data);

                    break;

                case ZoneProtocolReadState.Data:
                    // TODO: Figure out what to do with the data! Think this goes to a callback
                    //       in the ZNetworkManager and game client normally
                    //
                    //       For now just dump to file so that I can poke around in a hex editor
                    //
                    string nextFilename = string.Format("data_seq{0}", Sequence);

                    File.WriteAllBytes(nextFilename, buf);

                    Console.WriteLine(
                        "We received some data! I'm chucking it in {0}", nextFilename
                    );

                    // Test out checksum...
                    // the -4 is because the last DWORD signifies if this is a secure message, it's not
                    // part of the data
                    //
                    uint checksum = ZSecurity.GenerateChecksum(1, buf, LastMessageHeader.TotalSize - LastMessageHeader.ControlMsgSize - 4);

                    if (LastMessageHeader.IdOrKeyOrChecksum == checksum)
                    {
                        Console.WriteLine("Checksum good");
                    }
                    else
                    {
                        Console.WriteLine("Checksum bad, expected {0:X} and got {1:X}",
                            LastMessageHeader.IdOrKeyOrChecksum,
                            checksum);
                    }

                    SetReadState(ZoneProtocolReadState.DataHeader);
                    break;

                case ZoneProtocolReadState.FirstMsg:
                    // Don't think we care about this as a server?
                    break;
            }
        }

        private void SetReadState(ZoneProtocolReadState readState)
        {
            ReadState = readState;

            switch (ReadState)
            {
                case ZoneProtocolReadState.Hi:
                    NextBytesNeeded = ZMsgHi.DataSize;
                    break;

                case ZoneProtocolReadState.DataHeader:
                    NextBytesNeeded = ZMsgHeader.DataSize;
                    break;

                case ZoneProtocolReadState.Data:
                    NextBytesNeeded = LastMessageHeader.TotalSize - LastMessageHeader.ControlMsgSize;
                    break;

                case ZoneProtocolReadState.FirstMsg:
                    NextBytesNeeded = ZMsgFirst.DataSize;
                    break;
            }
        }
    }
}
