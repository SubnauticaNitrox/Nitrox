using System;
using NitroxModel.Logger;

namespace NitroxServer
{

    class Program
    {
        static void Main(string[] args)
        {
            ServerLogger log = new ServerLogger();
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug);

            try
            {
                Server _Server = new Server();
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
