using System;
using System.Net;
using System.Net.Sockets;

namespace GiderosPlayerRemote
{
    class MITM
    {
        static void Start(string[] args)
        {
            IPAddress listenAddr = IPAddress.Any;
            TcpListener listener = new TcpListener(listenAddr, 15001);
            listener.Start();
            Socket studio = listener.AcceptSocket();
            Console.WriteLine("accepted");

            Socket player = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            player.Connect(new IPEndPoint(IPAddress.Loopback, 15000));
            Console.WriteLine("connected");

            player.Blocking = false;
            studio.Blocking = false;

            while (true)
            {
                Pump(studio, player, "studio");
                Pump(player, studio, "player");
            }
        }

        static void Pump(Socket from, Socket to, string what)
        {
            var buffer = new byte[16];

            while (true)
            {
                SocketError serr;
                int rcvd = from.Receive(buffer, 0, buffer.Length, SocketFlags.None, out serr);
                if (serr == SocketError.WouldBlock) { return; }

                if (rcvd > 0)
                {
                    to.Send(buffer, rcvd, SocketFlags.None);

                    Console.Write("{0,8} ({1,2}) ", what, rcvd);

                    for (int i = 0; i < 16; ++i)
                    {
                        if (i % 4 == 0)
                        {
                            Console.Write(" ");
                        }
                        if (i < rcvd)
                            Console.Write("{0:X2} ", buffer[i]);
                        else
                            Console.Write("   ", buffer[i]);
                    }

                    Console.Write("| ");

                    for (int i = 0; i < 16; ++i)
                    {
                        char c = '.';
                        if (i < rcvd)
                        {
                            c = (char)buffer[i];
                            if (c < 32 || c > 126) c = '.';
                        }
                        Console.Write(c);
                    }

                    Console.WriteLine();
                }
                else
                {
                    throw new Exception("disconnected");
                }
            }
        }
    }
}
