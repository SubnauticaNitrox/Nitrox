using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;
using NitroxModel.Server;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal class GameModeCommand(IServerPacketSender packetSender, PlayerRepository playerRepository) : ICommandHandler<SubnauticaGameMode, ConnectedPlayerDto>
{
    private readonly IServerPacketSender packetSender = packetSender;
    private readonly PlayerRepository playerRepository = playerRepository;

    [Description("Changes a player's gamemode")]
    public async Task Execute(ICommandContext context, SubnauticaGameMode gameMode, ConnectedPlayerDto targetPlayer = null)
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
                if (!await playerRepository.SetPlayerGameMode(targetPlayer.Id, gameMode))
                {
                    await context.ReplyAsync($"Failed to set game mode of player {targetPlayer.Name} to {gameMode}");
                    break;
                }

                await packetSender.SendPacketToAll(GameModeChanged.ForPlayer(targetPlayer.SessionId, gameMode));
                await context.MessageAsync(targetPlayer.SessionId, $"GameMode changed to {gameMode}");
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
