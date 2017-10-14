using NitroxModel.Logger;
using System;

namespace NitroxServer
{
    class Program
    {
        static void Main(string[] args)
        {
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