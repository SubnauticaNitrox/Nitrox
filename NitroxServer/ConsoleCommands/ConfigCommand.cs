using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Serialization;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class ConfigCommand : Command
    {
        private readonly SemaphoreSlim configOpenLock = new(1);
        private readonly Server server;
        private readonly SubnauticaServerConfig serverConfig;

        public ConfigCommand(Server server, SubnauticaServerConfig serverConfig) : base("config", Perms.CONSOLE, "Opens the server configuration file")
        {
            this.server = server;
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
            string saveDir = Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), server.Name);
            string configFile = Path.Combine(saveDir, serverConfig.FileName);
            if (!File.Exists(configFile))
            {
                serverConfig.Serialize(saveDir);
            }

            Task.Run(async () =>
                {
                    try
                    {
                        await StartWithDefaultProgramAsync(configFile);
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

        private async Task StartWithDefaultProgramAsync(string fileToOpen)
        {
            using Process process = FileSystem.Instance.OpenOrExecuteFile(fileToOpen);
            await process.WaitForExitAsync();
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
