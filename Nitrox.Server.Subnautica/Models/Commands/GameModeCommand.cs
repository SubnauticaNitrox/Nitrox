using System.ComponentModel;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class GameModeCommand(PlayerService playerService) : ICommandHandler<SubnauticaGameMode, SessionId?>
{
    private readonly PlayerService playerService = playerService;

    [Description("Changes a player's gamemode")]
    public async Task Execute(ICommandContext context, SubnauticaGameMode gameMode, SessionId? targetSessionId = null)
    {
        switch (context.Origin)
        {
            case CommandOrigin.SERVER when targetSessionId == null:
                await context.ReplyAsync("Console can't use the gamemode command without providing a player name.");
                return;
            case CommandOrigin.PLAYER when context is PlayerToServerCommandContext playerContext:
                // The target player (if not set), is the player who sent the command.
                targetSessionId ??= playerContext.OriginId;
                goto default;
            default:
                if (targetSessionId == null)
                {
                    throw new ArgumentException("Target player must not be null");
                }

                playerService.GetProperty<GameModeProperty>(targetSessionId.Value).Value = gameMode;
                await context.SendToAllAsync(GameModeChanged.ForPlayer(targetSessionId.Value, gameMode));
                await context.SendAsync(targetSessionId.Value, $"GameMode changed to {gameMode}");
                string targetPlayerName = playerService.GetProperty<NameProperty>(targetSessionId.Value).Value;
                if (targetSessionId.Value != context.OriginId)
                {
                    await context.ReplyAsync($"GameMode of {targetPlayerName} #{targetSessionId.Value} changed to {gameMode}");
                }
                break;
        }
    }
}
