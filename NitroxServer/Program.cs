using System;
using NitroxModel.Logger;
using NitroxServer.ConfigParser;


namespace NitroxServer
{
    static class Program
    {
        static void Main(string[] args)
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug);

            try
            {
                ServerConfig config = new ServerConfig();
                Server server = new Server(config);
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
