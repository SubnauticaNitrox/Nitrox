using System;
using NitroxModel.Logger;

namespace NitroxServer
{

    class Program
    {
        static void Main(string[] args)
        {
            StaticLogger.Instance = new Log(LogLevels.All, Console.Out);
            StaticLogger.Instance.Debug($"Log levels set to: {StaticLogger.Instance.AllowedLevels}");

            try
            {
                Server server = new Server();
                server.Start();
            }
            catch (Exception e)
            {
                StaticLogger.Instance.Error(e);
            }

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
