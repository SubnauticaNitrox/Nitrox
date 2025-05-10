using System.ComponentModel;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("exit", "halt", "quit", "close")]
[RequiresPermission(Perms.ADMIN)]
internal sealed class StopCommand(IHostApplicationLifetime lifetime) : ICommandHandler
{
    private readonly IHostApplicationLifetime lifetime = lifetime;

    [Description("Gracefully stops the server")]
    public Task Execute(ICommandContext context)
    {
        lifetime.StopApplication();
        return Task.CompletedTask;
    }
}
