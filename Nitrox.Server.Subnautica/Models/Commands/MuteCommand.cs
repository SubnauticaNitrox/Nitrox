using System.ComponentModel;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class MuteCommand(PlayerService playerService) : ICommandHandler<SessionId>
{
    private readonly PlayerService playerService = playerService;

    public async Task Execute(ICommandContext context, [Description("Player to mute")] SessionId targetSessionId)
    {
        if (context.OriginId == targetSessionId)
        {
            await context.ReplyAsync("You can't mute yourself");
            return;
        }
        string targetName = playerService.GetProperty<NameProperty>(targetSessionId).Value;
        if (context.Permissions <= playerService.GetProperty<PermissionsProperty>(targetSessionId).Value)
        {
            await context.ReplyAsync($"You're not allowed to mute {targetName}");
            return;
        }
        IsMutedProperty targetMutedProperty = playerService.GetProperty<IsMutedProperty>(targetSessionId);
        if (targetMutedProperty.Value)
        {
            await context.ReplyAsync($"{targetName} is already muted");
            // Send state anyway in case it got desynced.
            await context.ReplyAsync(new MutePlayer(targetSessionId, true));
            return;
        }

        targetMutedProperty.Value = true;
        await context.SendToAllAsync(new MutePlayer(targetSessionId, targetMutedProperty.Value));
        await context.SendAsync(targetSessionId, "You're now muted");
        await context.ReplyAsync($"Muted {targetName}");
    }
}
