using System;
using NitroxModel.Logger;

namespace NitroxServer
{
    static class Program
    {
        static void Main(string[] args)
        {
            ServerLogger log = new ServerLogger();
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug);

            try
            {
                Server server = new Server();
                server.Start();
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
