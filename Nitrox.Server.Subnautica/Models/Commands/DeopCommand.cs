using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class DeopCommand(PlayerManager playerManager) : ICommandHandler<Player>
{
    private const Perms DEOP_PERMS_DEFAULT = Perms.PLAYER;
    private readonly PlayerManager playerManager = playerManager;

    [Description("Removes admin rights from user")]
    public async Task Execute(ICommandContext context, [Description("Username to remove admin rights from")] Player targetPlayer)
    {
        switch (context)
        {
            case not null when targetPlayer.Id == context.OriginId:
                await context.ReplyAsync("You can't deop yourself!");
                break;
            case not null when targetPlayer.Permissions >= context.Permissions:
                await context.ReplyAsync($"You're not allowed to remove admin permissions of {targetPlayer.Name}");
                break;
            default:
                if (!playerManager.SetPlayerProperty(targetPlayer.SessionId, DEOP_PERMS_DEFAULT, (player, perms) => player.Permissions = perms))
                {
                    await context.ReplyAsync($"Failed to change permissions to {DEOP_PERMS_DEFAULT}");
                    break;
                }
                await context.SendAsync(targetPlayer.SessionId, new PermsChanged(DEOP_PERMS_DEFAULT)); // Notify so they no longer get admin stuff on client (which would in any way stop working)
                await context.SendAsync(targetPlayer.SessionId, $"You were demoted to {DEOP_PERMS_DEFAULT}");
                await context.ReplyAsync($"Updated {targetPlayer.Name}'s permissions to {DEOP_PERMS_DEFAULT}");
                break;
        }
    }
}
