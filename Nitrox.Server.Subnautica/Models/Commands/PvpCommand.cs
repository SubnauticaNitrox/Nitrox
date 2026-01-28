using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class PvpCommand(IOptions<SubnauticaServerOptions> options) : ICommandHandler<bool>
{
    private readonly IOptions<SubnauticaServerOptions> options = options;

    [Description("Enables/Disables PvP")]
    public async Task Execute(ICommandContext context, bool state)
    {
        if (options.Value.PvpEnabled == state)
        {
            await context.ReplyAsync($"PvP is already {state}");
            return;
        }

        options.Value.PvpEnabled = state;
        await context.SendToAllAsync($"PvP is now {state}");
    }
}
