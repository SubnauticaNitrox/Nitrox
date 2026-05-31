using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class WarpCommand(IPacketSender packetSender) : ICommandHandler<Player>, ICommandHandler<Player, Player>
{
    private readonly IPacketSender packetSender = packetSender;

    [RequiresOrigin(CommandOrigin.PLAYER)]
    [Description("Teleports you to the target player")]
    public async Task Execute(ICommandContext context, [Description("Player to teleport to")] Player targetPlayer)
    {
        if (context is not PlayerToServerCommandContext playerContext)
        {
            return;
        }

        playerContext.Player.Teleport(targetPlayer.Position, targetPlayer.SubRootId, packetSender);
        await context.ReplyAsync($"Teleported to {targetPlayer.Name}");
    }

    [Description("Teleports first player to the second player")]
    public async Task Execute(ICommandContext context, Player warpingPlayer, Player targetPlayer)
    {
        warpingPlayer.Teleport(targetPlayer.Position, targetPlayer.SubRootId, packetSender);
        await context.ReplyAsync($"Teleported to {targetPlayer.Name}");
    }
}
