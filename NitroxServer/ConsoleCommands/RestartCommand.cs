using System.Diagnostics;
using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal class RestartCommand : Command
    {
        private readonly Server server;
        private readonly ServerConfig serverConfig;

        public RestartCommand(Server server, ServerConfig serverConfig) : base("restart", Perms.CONSOLE, "Restarts the server")
        {
            AddParameter(new TypeBoolean("reset", false));
            
            this.server = server;
            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            string program = Process.GetCurrentProcess().MainModule?.FileName;
            if (program == null)
            {
                Log.Error("Failed to get location of server.");
                return;
            }

            SendMessageToAllPlayers("Server is restarting...");
            server.Stop();
            // If reset, delete save.
            if (args.Get<bool>(0))
            {
                if (Directory.Exists(serverConfig.SaveName))
                {
                    Directory.Delete(serverConfig.SaveName, true);
                }
            }
            using Process proc = Process.Start(program);
        }
    }
}
