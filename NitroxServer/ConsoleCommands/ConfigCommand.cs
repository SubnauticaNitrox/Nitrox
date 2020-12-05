using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Serialization;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal sealed class ConfigCommand : Command
    {
        private readonly SemaphoreSlim configOpenLock = new SemaphoreSlim(1);

        public ConfigCommand() : base("config", Perms.CONSOLE, "Opens the server configuration file")
        {
        }

        protected override void Execute(CallArgs args)
        {
            if (!configOpenLock.Wait(0))
            {
                Log.Warn("Waiting on previous config command to close the configuration file.");
                return;
            }

            ServerConfig currentActiveConfig = NitroxServiceLocator.LocateService<ServerConfig>();
            string configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "", currentActiveConfig.FileName);
            if (!File.Exists(configFile))
            {
                Log.Error($"Could not find config file at: {configFile}");
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    await StartProcessAsync("notepad", configFile);
                }
                finally
                {
                    configOpenLock.Release();
                }
                ServerConfig newConfigFromFile = PropertiesWriter.Deserialize<ServerConfig>();
                if (!ServerConfig.ServerConfigComparer.Equals(currentActiveConfig, newConfigFromFile))
                {
                    Log.Info("Config file has changed. Restart the server to make the changes take effect!");
                }
            });
        }

        private async Task StartProcessAsync(string fileName, string arguments)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = fileName;
            info.Arguments = arguments;
            info.UseShellExecute = false;
            using Process process = Process.Start(info);
            while (!process?.HasExited == false)
            {
                await Task.Delay(100);
            }
        }
    }
}
