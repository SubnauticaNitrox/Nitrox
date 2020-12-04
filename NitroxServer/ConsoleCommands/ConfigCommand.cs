using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Serialization;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal class ConfigCommand : Command
    {
        public ConfigCommand() : base("config", Perms.CONSOLE, "Opens the server configuration file")
        {
        }

        protected override void Execute(CallArgs args)
        {
            ServerConfig currentActiveConfig = NitroxServiceLocator.LocateService<ServerConfig>();
            string configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "", currentActiveConfig.FileName);
            if (!File.Exists(configFile))
            {
                Log.Error($"Could not find config file at: {configFile}");
                return;
            }

            Task.Run(async () =>
            {
                await StartProcessAsync("notepad", configFile);
                ServerConfig newConfigFromFile = PropertiesWriter.Deserialize<ServerConfig>();
                if (!ServerConfig.ServerConfigComparer.Equals(currentActiveConfig, newConfigFromFile))
                {
                    Log.Info("Config file has changed. Restart the server to make the changes take effect!");
                }
            });
        }

        private Task StartProcessAsync(string fileName, string arguments)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = fileName;
            info.Arguments = arguments;
            info.UseShellExecute = false;
            using Process process = Process.Start(info);
            process?.WaitForExit();
            return Task.CompletedTask;
        }
    }
}
