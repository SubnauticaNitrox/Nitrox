using System.ComponentModel;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class KickCommand(IKickPlayer playerKicker, PlayerService playerService) : ICommandHandler<SessionId, string>
{
    private readonly IKickPlayer playerKicker = playerKicker;
    private readonly PlayerService playerService = playerService;

    [Description("Kicks a player from the server")]
    public async Task Execute(ICommandContext context, SessionId playerToKick, string reason = "")
    {
        if (context.OriginId == playerToKick)
        {
            await context.ReplyAsync("You can't kick yourself");
            return;
        }

        switch (context.Origin)
        {
            case CommandOrigin.PLAYER when playerService.GetProperty<PermissionsProperty>(playerToKick).Value >= context.Permissions:
                await context.ReplyAsync($"You're not allowed to kick {playerService.GetProperty<NameProperty>(playerToKick).Value} #{playerToKick}");
                break;
            default:
                if (!await playerKicker.KickPlayer(playerToKick, reason))
                {
                    await context.ReplyAsync($"Failed to kick '{playerService.GetProperty<NameProperty>(playerToKick).Value}'");
                }
                break;
        }
    }
}
