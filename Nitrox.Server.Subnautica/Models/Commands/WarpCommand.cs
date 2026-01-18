using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class WarpCommand : ICommandHandler<Player>, ICommandHandler<Player, Player>
{
    [RequiresOrigin(CommandOrigin.PLAYER)]
    [Description("Teleports you to the target player")]
    public async Task Execute(ICommandContext context, [Description("Player to teleport to")] Player targetPlayer)
    {
        if (context is not PlayerToServerCommandContext playerContext)
        {
            return;
        }

        playerContext.Player.Teleport(targetPlayer.Position, targetPlayer.SubRootId);
        await context.ReplyAsync($"Teleported to {targetPlayer.Name}");
    }

    [Description("Teleports first player to the second player")]
    public async Task Execute(ICommandContext context, Player warpingPlayer, Player targetPlayer)
    {
        warpingPlayer.Teleport(targetPlayer.Position, targetPlayer.SubRootId);
        await context.ReplyAsync($"Teleported to {targetPlayer.Name}");
    }
}
