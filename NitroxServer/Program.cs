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
                ServerConfigReader GetConfig = new ServerConfigReader();
                GetConfig.ReadServerConfig(@".\config.ini");
                Server server = new Server(GetConfig);
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
