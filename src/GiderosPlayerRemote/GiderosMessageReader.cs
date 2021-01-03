using System;
using System.IO;
using System.Net.Sockets;

namespace GiderosPlayerRemote
{
    class GiderosMessageReader
    {
        Socket soc;
        byte[] buffer;
        int bufferEnd;

        public GiderosMessageReader(Socket soc)
        {
            this.soc = soc;
            this.buffer = new byte[1000];
        }

        public int ReceiveMore()
        {
            // 절반 이상 차면 두 배로
            if (bufferEnd >= buffer.Length / 2)
            {
                byte[] newBuf = new byte[buffer.Length * 2];
                Array.Copy(buffer, newBuf, bufferEnd);
                buffer = newBuf;
            }

            // 받는다
            SocketError serr;
            int rcvd = soc.Receive(
                buffer,
                bufferEnd,
                buffer.Length - bufferEnd,
                SocketFlags.None,
                out serr);
            if (serr == SocketError.Success)
            {
                /*
                Console.WriteLine();
                for (int i = 0; i < rcvd; ++i)
                {
                    Console.Write("{0:X2} ", buffer[i + bufferEnd]);
                }
                Console.WriteLine();
                */
                bufferEnd += rcvd;
                return rcvd;
            }
            else
            {
                throw new Exception(serr.ToString());
            }
        }

        public ReceivedGiderosMessage TryTakeMessageFromBuffer()
        {
            while (true)
            {
                if (bufferEnd < 12)
                {
                    return null;
                }

                using (var strm = new MemoryStream(buffer, 0, bufferEnd, false))
                using (var reader = new BinaryReader(strm))
                {
                    int msgLength = reader.ReadInt32();
                    int seqNum = reader.ReadInt32();
                    int msgType = reader.ReadInt32();

                    if (bufferEnd < msgLength)
                    {
                        return null;
                    }

                    var rv = new byte[msgLength - 12];
                    Array.Copy(
                        buffer, 12,
                        rv, 0,
                        msgLength - 12);
                    Array.Copy(
                        buffer, msgLength,
                        buffer, 0,
                        bufferEnd - msgLength);
                    bufferEnd -= msgLength;

                    if (msgType == 1) // 0=>we are sending data, 1=>ack)
                    {
                        continue;
                    }
                    else
                    {
                        return new ReceivedGiderosMessage(rv);
                    }
                }
            }
        }
    }
}
