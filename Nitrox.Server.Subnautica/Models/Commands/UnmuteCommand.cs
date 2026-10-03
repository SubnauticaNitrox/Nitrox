using System.ComponentModel;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class UnmuteCommand(PlayerService playerService) : ICommandHandler<SessionId>
{
    private readonly PlayerService playerService = playerService;

    [Description("Removes a mute from a player")]
    public async Task Execute(ICommandContext context, [Description("Player to unmute")] SessionId targetPlayer)
    {
        if (context.OriginId == targetPlayer)
        {
            await context.ReplyAsync("You can't unmute yourself");
            return;
        }
        if (playerService.GetProperty<PermissionsProperty>(targetPlayer).Value >= context.Permissions)
        {
            await context.ReplyAsync($"You're not allowed to unmute {playerService.GetProperty<NameProperty>(targetPlayer).Value}");
            return;
        }
        IsMutedProperty targetIsMutedProperty = playerService.GetProperty<IsMutedProperty>(targetPlayer);
        if (!targetIsMutedProperty.Value)
        {
            await context.ReplyAsync($"{playerService.GetProperty<NameProperty>(targetPlayer).Value} is already unmuted");
            await context.ReplyAsync(new MutePlayer(targetPlayer, false));
            return;
        }

        targetIsMutedProperty.Value = false;
        await context.SendToAllAsync(new MutePlayer(targetPlayer, targetIsMutedProperty.Value));
        await context.SendAsync(targetPlayer, "You're no longer muted");
        await context.ReplyAsync($"Unmuted {playerService.GetProperty<NameProperty>(targetPlayer).Value}");
    }
}
