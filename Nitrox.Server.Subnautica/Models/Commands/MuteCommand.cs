using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class MuteCommand : ICommandHandler<Player>
{
    public async Task Execute(ICommandContext context, [Description("Player to mute")] Player targetPlayer)
    {
        if (context.OriginId == targetPlayer.SessionId)
        {
            await context.ReplyAsync("You can't mute yourself");
            return;
        }
        if (context.Permissions <= targetPlayer.Permissions)
        {
            await context.ReplyAsync($"You're not allowed to mute {targetPlayer.Name}");
            return;
        }

        if (targetPlayer.PlayerContext.IsMuted)
        {
            await context.ReplyAsync($"{targetPlayer.Name} is already muted");
            // Send state anyway in case it got desynced.
            await context.ReplyAsync(new MutePlayer(targetPlayer.SessionId, targetPlayer.PlayerContext.IsMuted));
            return;
        }

        targetPlayer.PlayerContext.IsMuted = true;
        await context.SendToAllAsync(new MutePlayer(targetPlayer.SessionId, targetPlayer.PlayerContext.IsMuted));
        await context.SendAsync(targetPlayer.SessionId, "You're now muted");
        await context.ReplyAsync($"Muted {targetPlayer.Name}");
    }
}
