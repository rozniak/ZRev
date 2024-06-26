using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ZSrv.ControlMessages;
using ZSrv.RoutMessages;

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
                    string nextFilename = string.Format("data_seq{0}", DateTime.UtcNow.Ticks);

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

                        // Data analysis
                        //
                        ZMsgData msgData = new ZMsgData(LastMessageHeader, buf);

                        Console.WriteLine(
                            "Data context: {0}",
                            Encoding.ASCII.GetString(BitConverter.GetBytes(msgData.DataHeader.Identifier))
                        );
                        Console.WriteLine(
                            "Context: {0}",
                            msgData.DataHeader.Context
                        );
                        Console.WriteLine(
                            "Payload size: {0}",
                            msgData.DataHeader.PayloadSize
                        );

                        HandleData(ns, msgData);
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

        private void HandleData(NetworkStream ns, ZMsgData msgData)
        {
            if (msgData.DataHeader.Identifier == 0x726f7574) // rout message
            {
                // TODO: Handle this properly...
                Console.WriteLine("rout message received - sending hello");

                ZRoutMsg msgRout = new ZRoutMsg();

                msgRout.Chunks.Add(new ZRoutHelloChunk());
                msgRout.Chunks.Add(new ZRoutAbilitiesChunk());
                msgRout.Chunks.Add(new ZRoutProxyInfoChunk("mchkr_zm_***")); // Tee-hee

                // Create the data packet
                //
                var newData = new ZMsgData();
                
                newData.Payload = msgRout.GetBytes();

                newData.Header.Magic = ZMsgHeader.Signature;
                newData.Header.TotalSize = newData.DataSize;
                newData.Header.Sequence = ++Sequence;
                newData.Header.ControlMsgSize = (ushort) ZMsgHeader.DataSize;
                newData.Header.Unknown0xc = 0;
                // Checksum set by data fill bytes

                newData.DataHeader.Identifier = 0x726f7574;
                newData.DataHeader.Context = 3; // FIXME: Should be an enum once we know it
                newData.DataHeader.PayloadSize = (uint) newData.Payload.Length;

                // Fill
                //
                byte[] newBuf = new byte[newData.DataSize];

                newData.FillBuffer(newBuf);

                ns.Write(newBuf, 0, newBuf.Length);

                // TESTING
                //File.WriteAllBytes("resp" + DateTime.UtcNow.Ticks.ToString(), newBuf);

                uint checksum = ZSecurity.GenerateChecksum(1, newBuf, newData.Header.TotalSize - newData.Header.ControlMsgSize - 4, ZMsgHeader.DataSize);

                Console.WriteLine("Our checksum out: 0x{0:X} vs 0x{1:X}", newData.Header.IdOrKeyOrChecksum, checksum);
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
