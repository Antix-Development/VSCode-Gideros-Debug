using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace VSCodeDebug
{
    class DebuggeeProtocol : IDebuggeeSender
    {
        IDebuggeeListener debuggeeListener;
        NetworkStream networkStream;
        ByteBuffer recvBuffer = new ByteBuffer();
        Encoding encoding;
        TcpListener tcpListener;

        public DebuggeeProtocol(
            IDebuggeeListener debuggeeListener,
            TcpListener tcpListener,
            Encoding encoding)
        {
            this.debuggeeListener = debuggeeListener;
            this.tcpListener = tcpListener;
            this.encoding = encoding;
        }

        public void StartThread()
        {
            new System.Threading.Thread(() => SocketStreamLoop()).Start();
        }

        void SocketStreamLoop()
        {
            try
            {
                var clientSocket = tcpListener.AcceptSocket(); // blocked here
                networkStream = new NetworkStream(clientSocket);
                tcpListener.Stop();
                tcpListener = null;

                debuggeeListener.X_DebuggeeArrived(this);

                while (true)
                {
                    var buffer = new byte[10000];
                    var read = networkStream.Read(buffer, 0, buffer.Length);

                    if (read == 0) { break; } // end of stream
                    if (read > 0)
                    {
                        recvBuffer.Append(buffer, read);
                        while (ProcessData()) { }
                    }
                }
            }
            catch (Exception /*e*/)
            {
                //Program.MessageBox(IntPtr.Zero, e.ToString(), "LuaDebug", 0);
            }

            debuggeeListener.X_DebuggeeHasGone();
        }

        bool ProcessData()
        {
            string s = recvBuffer.GetString(encoding);
            int headerEnd = s.IndexOf('\n');
            if (headerEnd < 0) { return false; }

            string header = s.Substring(0, headerEnd);
            if (header[0] != '#') { throw new Exception("Broken header:" + header); }
            var bodySize = int.Parse(header.Substring(1));

            // 헤더는 모두 0~127 아스키 문자로만 이루어지기 때문에
            // 문자열 길이로 계산했을 때와 바이트 개수로 계산했을 때의 결과가 같다.
            if (recvBuffer.Length < headerEnd + 1 + bodySize) { return false; }

            recvBuffer.RemoveFirst(headerEnd + 1);
            byte[] bodyBytes = recvBuffer.RemoveFirst(bodySize);

            string body = encoding.GetString(bodyBytes);
            //MessageBox.OK(body);

            debuggeeListener.X_FromDebuggee(bodyBytes);
            return true;
        }

        void IDebuggeeSender.Send(string reqText)
        {
            byte[] bodyBytes = encoding.GetBytes(reqText);
            string header = '#' + bodyBytes.Length.ToString() + "\n";
            byte[] headerBytes = encoding.GetBytes(header);
            try
            {
                networkStream.Write(headerBytes, 0, headerBytes.Length);
                networkStream.Write(bodyBytes, 0, bodyBytes.Length);
            }
            catch (IOException)
            {
                debuggeeListener.X_DebuggeeHasGone();
            }
        }
    }
}
