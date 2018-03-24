using System;
using NitroxModel.Logger;
using NitroxClient.GameLogic.ChatUI;

namespace NitroxServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.SetLevel(Log.LogLevel.CONSOLE_INFO | Log.LogLevel.CONSOLE_DEBUG | Log.LogLevel.IN_GAME_MESSAGES);

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
