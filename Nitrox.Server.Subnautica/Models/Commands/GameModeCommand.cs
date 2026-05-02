using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class GameModeCommand : ICommandHandler<SubnauticaGameMode, Player>
{
    [Description("Changes a player's gamemode")]
    public async Task Execute(ICommandContext context, SubnauticaGameMode gameMode, Player? targetPlayer = null)
    {
        switch (context.Origin)
        {
            case CommandOrigin.SERVER when targetPlayer == null:
                await context.ReplyAsync("Console can't use the gamemode command without providing a player name.");
                return;
            case CommandOrigin.PLAYER when context is PlayerToServerCommandContext playerContext:
                // The target player (if not set), is the player who sent the command.
                targetPlayer ??= playerContext.Player;
                goto default;
            default:
                if (targetPlayer == null)
                {
                    throw new ArgumentException("Target player must not be null");
                }

                targetPlayer.GameMode = gameMode;
                await context.SendToAllAsync(GameModeChanged.ForPlayer(targetPlayer.SessionId, gameMode));
                await context.SendAsync(targetPlayer.SessionId, $"GameMode changed to {gameMode}");
                if (context.Origin == CommandOrigin.SERVER)
                {
                    await context.ReplyAsync($"Changed {targetPlayer.Name} [{targetPlayer.Id}]'s gamemode to {gameMode}");
                }
                else if (targetPlayer.Id != context.OriginId)
                {
                    await context.ReplyAsync($"GameMode of {targetPlayer.Name} changed to {gameMode}");
                }
                break;
        }
    }
}
