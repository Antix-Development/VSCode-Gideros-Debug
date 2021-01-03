using System;
using System.Text;

namespace GiderosPlayerRemote
{
    class ReceivedGiderosMessage
    {
        byte[] buffer;
        int offset = 0;

        public ReceivedGiderosMessage(byte[] b)
        {
            buffer = b;
        }

        public byte ReadByte()
        {
            return buffer[offset++];
        }

        public string ReadString()
        {
            for (int i = offset; i < buffer.Length; ++i)
            {
                if (buffer[i] == 0)
                {
                    string rv = Encoding.UTF8.GetString(buffer, offset, i - offset);
                    offset = i + 1;
                    return rv;
                }
            }

            throw new Exception("no '\0' found");
        }

        public int ReadInt()
        {
            int rv = BitConverter.ToInt32(buffer, offset);
            offset += 4;
            return rv;
        }

        public byte[] ReadBytes(int size)
        {
            var rv = new byte[size];
            Array.Copy(
                buffer, offset,
                rv, 0,
                size);
            offset += size;
            return rv;
        }

        public bool IsEOB()
        {
            return offset == buffer.Length;
        }
    }
}
