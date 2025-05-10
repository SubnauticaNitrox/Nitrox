using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresOrigin(CommandOrigin.SERVER)]
internal sealed class RestartCommand(IOptions<ServerStartOptions> optionsProvider, RestartService restartService, ILogger<RestartCommand> logger, IHostApplicationLifetime lifetime) : ICommandHandler
{
    private readonly IOptions<ServerStartOptions> optionsProvider = optionsProvider;
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
        if (optionsProvider.Value.IsEmbedded)
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
