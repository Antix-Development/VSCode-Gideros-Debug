using System;

namespace GiderosPlayerRemote
{
    class Program
    {
        class Logger : IRemoteControllerListener
        {
            void IRemoteControllerListener.X_Log(LogType logType, string content)
            {
                Console.WriteLine(logType + " " + content);
            }
        }

        static void Main(string[] args)
        {
            var rc = new RemoteController();
            //rc.Run("127.0.0.1", 15000, @"C:\dev\VSCodeLuaDebug\debuggee\gideros.gproj");
            if (rc.TryStart("127.0.0.1", 15000, @"C:\dev\MTCG\MTCG.gproj", new Logger()))
            {
                rc.ReadLoop();
            }
            else
            {
                Console.WriteLine("connection failed");
            }
        }
    }
}
