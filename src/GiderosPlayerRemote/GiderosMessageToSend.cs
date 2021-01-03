using System.IO;
using System.Net.Sockets;
using System.Text;

namespace GiderosPlayerRemote
{
    class GiderosMessageToSend
    {
        int seqId;
        MemoryStream bodyStream;
        BinaryWriter bodyWriter;
        NetworkStream netStream;

        public GiderosMessageToSend(int seqId, NetworkStream netStream)
        {
            this.seqId = seqId;
            this.netStream = netStream;
            this.bodyStream = new MemoryStream();
            this.bodyWriter = new BinaryWriter(bodyStream);
        }

        public void Send()
        {
            var bytes = ToBytes();
            netStream.Write(bytes, 0, bytes.Length);
        }

        byte[] ToBytes()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                byte[] payload = bodyStream.ToArray();

                writer.Write((int)payload.Length + 12);
                writer.Write(seqId);
                writer.Write((int)0); // 0=msg, 1=ack
                writer.Write(payload);

                return stream.ToArray();
            }
        }

        public GiderosMessageToSend AppendByte(byte by)
        {
            bodyWriter.Write(by);
            return this;
        }

        public GiderosMessageToSend AppendInt(int n)
        {
            bodyWriter.Write(n);
            return this;
        }

        public GiderosMessageToSend AppendFloat(float n)
        {
            bodyWriter.Write(n);
            return this;
        }

        public GiderosMessageToSend AppendString(string s)
        {
            bodyWriter.Write(Encoding.UTF8.GetBytes(s));
            bodyWriter.Write((byte)0);
            return this;
        }

        public GiderosMessageToSend AppendByteArray(byte[] ba)
        {
            bodyWriter.Write(ba);
            return this;
        }
    }
}
