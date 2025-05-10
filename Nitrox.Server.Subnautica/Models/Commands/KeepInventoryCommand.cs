using System.ComponentModel;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal class KeepInventoryCommand(IServerPacketSender packetSender, IOptions<Configuration.SubnauticaServerOptions> optionsProvider) : ICommandHandler<bool>
{
    private readonly IOptions<Configuration.SubnauticaServerOptions> serverConfig = optionsProvider;

    [Description("Sets \"keep inventory\" setting to on/off. If \"on\", players won't lose items when they die.")]
    public async Task Execute(ICommandContext context, [Description("The true/false state to set keep inventory on death to")] bool newKeepInventoryState)
    {
        if (serverConfig.Value.KeepInventoryOnDeath != newKeepInventoryState)
        {
            serverConfig.Value.KeepInventoryOnDeath = newKeepInventoryState;
            await packetSender.SendPacketToAll(new KeepInventoryChanged(newKeepInventoryState));
            await context.MessageAllAsync($"KeepInventoryOnDeath changed to \"{newKeepInventoryState}\" by {context.OriginName}");
        }
        else
        {
            await context.ReplyAsync($"KeepInventoryOnDeath already set to {newKeepInventoryState}");
        }
    }
}
