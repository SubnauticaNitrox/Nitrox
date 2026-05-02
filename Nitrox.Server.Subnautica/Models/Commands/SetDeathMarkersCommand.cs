using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class SetDeathMarkersCommand(IOptions<SubnauticaServerOptions> options) : ICommandHandler<bool> {
    private readonly IOptions<SubnauticaServerOptions> options = options;

    [Description("Sets \"death markers\" setting to on/off. If \"on\", a beacon will be placed when a player dies at the location of the death.")]
    public async Task Execute(ICommandContext context, [Description("The true/false state to set the death markers setting to")] bool newState)
    {
        if(options.Value.MarkDeathPointsWithBeacon == newState)
        {
            await context.ReplyAsync($"MarkDeathPointsWithBeacon already set to {newState}");
            return;
        }
        options.Value.MarkDeathPointsWithBeacon = newState;
        await context.SendToAllAsync(new DeathMarkersChanged(newState));
        await context.SendToAllAsync($"MarkDeathPointsWithBeacon changed to \"{newState}\" by {context.OriginName}");
    }
}
