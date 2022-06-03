using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Platforms.OS.Shared;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands
{
    internal class ConfigCommand : Command
    {
        private readonly SemaphoreSlim configOpenLock = new(1);
        private readonly ServerConfig serverConfig;

        public ConfigCommand(ServerConfig serverConfig) : base("config", Perms.CONSOLE, "Opens the server configuration file")
        {
            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            if (!configOpenLock.Wait(0))
            {
                Log.Warn("Waiting on previous config command to close the configuration file.");
                return;
            }

            // Save config file if it doesn't exist yet.
            string saveDir = Path.Combine(WorldManager.SavesFolderDir, serverConfig.SaveName);
            string configFile = Path.Combine(saveDir, serverConfig.FileName);
            if (!File.Exists(configFile))
            {
                serverConfig.Serialize(configFile);
            }

            Task.Run(async () =>
                {
                    try
                    {
                        await StartWithDefaultProgram(configFile);
                    }
                    finally
                    {
                        configOpenLock.Release();
                    }
                    serverConfig.Deserialize(saveDir); // Notifies user if deserialization failed.
                    Log.Info("If you made changes, restart the server for them to take effect.");
                })
                .ContinueWith(t =>
                {
#if DEBUG
                    if (t.Exception != null)
                    {
                        throw t.Exception;
                    }
#endif
                });
        }

        private async Task StartWithDefaultProgram(string fileToOpen)
        {
            using Process process = FileSystem.Instance.OpenOrExecuteFile(fileToOpen);
            process.WaitForExit();
            try
            {
                while (!process.HasExited)
                {
                    await Task.Delay(100);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
