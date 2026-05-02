using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class SetKeepInventoryCommand(IOptions<SubnauticaServerOptions> options) : ICommandHandler<bool>
{
    private readonly IOptions<SubnauticaServerOptions> options = options;

    [Description("Sets \"keep inventory\" setting to on/off. If \"on\", players won't lose items when they die.")]
    public async Task Execute(ICommandContext context, [Description("The true/false state to set keep inventory on death to")] bool newState)
    {
        if (options.Value.KeepInventoryOnDeath == newState)
        {
            await context.ReplyAsync($"KeepInventoryOnDeath already set to {newState}");
            return;
        }

        options.Value.KeepInventoryOnDeath = newState;
        await context.SendToAllAsync(new KeepInventoryChanged(newState));
        await context.SendToAllAsync($"KeepInventoryOnDeath changed to \"{newState}\" by {context.OriginName}");
    }
}
