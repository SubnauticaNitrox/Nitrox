using System;
using NitroxModel.Logger;

namespace NitroxServer
{

    class Program
    {
        static void Main(string[] args)
        {
            StaticLogger.Instance = new Log(Log.LogLevels.Info | Log.LogLevels.Debug, Console.Out);

            try
            {
                Server _Server = new Server();
                _Server.Start();
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
