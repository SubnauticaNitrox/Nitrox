using System;
using NitroxModel.Logger;
using NitroxServer.ConfigParser;


namespace NitroxServer
{

    class Program
    {
        static void Main(string[] args)
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug);

            try
            {
                ServerConfigReader GetConfig = new ServerConfigReader();
                GetConfig.ReadServerConfig(@".\config.ini");
                Server _Server = new Server(GetConfig);
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
