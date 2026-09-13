using System.ComponentModel;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class DeopCommand(PlayerService playerService) : ICommandHandler<SessionId>
{
    private const Perms DEOP_PERMS_DEFAULT = Perms.PLAYER;
    private readonly PlayerService playerService = playerService;

    [Description("Removes admin rights from user")]
    public async Task Execute(ICommandContext context, [Description("Username to remove admin rights from")] SessionId targetPlayer)
    {
        PermissionsProperty targetPerms = playerService.GetProperty<PermissionsProperty>(targetPlayer);
        NameProperty targetName = playerService.GetProperty<NameProperty>(targetPlayer);
        switch (context)
        {
            case not null when targetPlayer == context.OriginId:
                await context.ReplyAsync("You can't deop yourself!");
                break;
            case not null when targetPerms.Value >= context.Permissions:
                await context.ReplyAsync($"You're not allowed to remove admin permissions of {targetName.Value}");
                break;
            default:
                targetPerms.Value = DEOP_PERMS_DEFAULT;
                await context.SendAsync(targetPlayer, new PermsChanged(DEOP_PERMS_DEFAULT)); // Notify so they no longer get admin stuff on client (which would in any way stop working)
                await context.SendAsync(targetPlayer, $"You were demoted to {DEOP_PERMS_DEFAULT}");
                await context.ReplyAsync($"Updated {targetName.Value}'s permissions to {DEOP_PERMS_DEFAULT}");
                break;
        }
    }
}
