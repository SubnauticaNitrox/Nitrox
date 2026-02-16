using System.ComponentModel;
using System.Diagnostics;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresOrigin(CommandOrigin.SERVER)]
internal sealed class RestartCommand(IOptions<ServerStartOptions> options, RestartService restartService, ILogger<RestartCommand> logger, IHostApplicationLifetime lifetime) : ICommandHandler
{
    private readonly IOptions<ServerStartOptions> options = options;
    private readonly IHostApplicationLifetime lifetime = lifetime;
    private readonly ILogger<RestartCommand> logger = logger;
    private readonly RestartService restartService = restartService;

    [Description("Restarts the server")]
    public Task Execute(ICommandContext context)
    {
        if (Debugger.IsAttached)
        {
            logger.ZLogError($"Server can not be restarted while a debugger is attached.");
            return Task.CompletedTask;
        }
        if (options.Value.IsEmbedded)
        {
            logger.ZLogError($"Use launcher to stop and start the server.");
            return Task.CompletedTask;
        }

        logger.ZLogInformation($"Server will restart on close. Stopping server...");
        restartService.RestartOnStop = true;
        lifetime.StopApplication();

        return Task.CompletedTask;
    }
}
