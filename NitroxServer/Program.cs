using System;
using NitroxModel.Logger;

namespace NitroxServer
{
    
    class Program
    {
        public static Server _Server;
        static void Main(string[] args)
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug);

            try
            {
                _Server = new Server();
                _Server.Start();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
