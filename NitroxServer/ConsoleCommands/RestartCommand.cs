using System;
using System.Diagnostics;
using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
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
                DeleteSaveFiles();
            }
            using Process proc = Process.Start(program);
        }

        private void DeleteSaveFiles()
        {
            string savePath = Path.GetFullPath(serverConfig.SaveName);
            if (!Directory.Exists(savePath))
            {
                return;
            }
            // Ensure save folder is inside Nitrox folder, as to not accidentally delete files in the wrong location.
            if (savePath.IndexOf(NitroxAppData.Instance.LauncherPath, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return;
            }
            
            foreach (string file in Directory.EnumerateFiles(savePath, "*", SearchOption.AllDirectories))
            {
                if (".zip".Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                        
                try
                {
                    File.Delete(file);
                }
                catch (IOException ex)
                {
                    Log.Warn($"Failed to purge a save file: {ex}");
                }
            }
        }
    }
}
