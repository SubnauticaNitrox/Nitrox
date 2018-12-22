using System;
using NitroxModel.Logger;
using NitroxServer.ConfigParser;
using NitroxServer.ConsoleCommands.Processor;


namespace NitroxServer
{
    public static class Program
    {
        public static bool IsRunning = true;
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

            while (IsRunning)
            {
                ConsoleCommandProcessor.ProcessCommand(Console.ReadLine());
            }
        }
    }
}
