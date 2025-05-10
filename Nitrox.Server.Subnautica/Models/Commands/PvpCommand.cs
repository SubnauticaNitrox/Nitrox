using System.ComponentModel;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal class PvpCommand(IOptions<SubnauticaServerConfig> configProvider) : ICommandHandler<bool>
{
    private readonly IOptions<SubnauticaServerConfig> configProvider = configProvider;

    [Description("Enables/Disables PvP")]
    public async Task Execute(ICommandContext context, bool state)
    {
        switch (context)
        {
            case not null when configProvider.Value.PvPEnabled == state:
                await context.ReplyAsync($"PvP is already {state}");
                break;
            case not null:
                configProvider.Value.PvPEnabled = state; // TODO: Ensure it's persisted
                await context.MessageAllAsync($"PvP is now {state}");
                break;
        }
    }
}
