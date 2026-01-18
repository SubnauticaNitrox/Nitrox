using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class UnmuteCommand : ICommandHandler<Player>
{
    [Description("Removes a mute from a player")]
    public async Task Execute(ICommandContext context, [Description("Player to unmute")] Player targetPlayer)
    {
        if (context.OriginId == targetPlayer.SessionId)
        {
            await context.ReplyAsync("You can't unmute yourself");
            return;
        }
        if (targetPlayer.Permissions >= context.Permissions)
        {
            await context.ReplyAsync($"You're not allowed to unmute {targetPlayer.Name}");
            return;
        }
        if (!targetPlayer.PlayerContext.IsMuted)
        {
            await context.ReplyAsync($"{targetPlayer.Name} is already unmuted");
            await context.ReplyAsync(new MutePlayer(targetPlayer.SessionId, false));
            return;
        }

        targetPlayer.PlayerContext.IsMuted = false;
        await context.SendToAllAsync(new MutePlayer(targetPlayer.SessionId, targetPlayer.PlayerContext.IsMuted));
        await context.SendAsync(targetPlayer.SessionId, "You're no longer muted");
        await context.ReplyAsync($"Unmuted {targetPlayer.Name}");
    }
}
