using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("exit", "halt", "quit", "close")]
[RequiresPermission(Perms.ADMIN)]
internal sealed class StopCommand(IHostApplicationLifetime lifetimeService) : ICommandHandler
{
    private readonly IHostApplicationLifetime lifetimeService = lifetimeService;

    [Description("Stops the server")]
    public Task Execute(ICommandContext context)
    {
        lifetimeService.StopApplication();
        return Task.CompletedTask;
    }
}
