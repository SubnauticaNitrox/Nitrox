using System.Diagnostics;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Platforms.OS.Shared;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class ConfigCommand : Command
    {
        private readonly SemaphoreSlim configOpenLock = new(1);
        private readonly IOptions<ServerStartOptions> options;
        private readonly ILogger<ConfigCommand> logger;

        public ConfigCommand(IOptions<ServerStartOptions> options, ILogger<ConfigCommand> logger) : base("config", Perms.HOST, "Opens the server configuration file")
        {
            this.options = options;
            this.logger = logger;
        }

        protected override void Execute(CallArgs args)
        {
            if (!configOpenLock.Wait(0))
            {
                logger.ZLogWarning($"Waiting on previous config command to close the configuration file.");
                return;
            }

            Task.Run(async () =>
                {
                    try
                    {
                        await StartWithDefaultProgramAsync(options.Value.GetServerConfigFilePath());
                    }
                    finally
                    {
                        configOpenLock.Release();
                    }
                    logger.ZLogInformation($"If you made changes, restart the server for them to take effect.");
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
