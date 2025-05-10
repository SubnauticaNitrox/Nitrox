using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal class MuteCommand(PlayerRepository playerRepository) : ICommandHandler<ConnectedPlayerDto>
{
    private readonly PlayerRepository playerRepository = playerRepository;

    [Description("Prevents a user from chatting")]
    public async Task Execute(ICommandContext context, [Description("Player to mute")] ConnectedPlayerDto targetPlayer)
    {
        switch (context)
        {
            case not null when context.OriginId == targetPlayer.Id:
                await context.ReplyAsync("You can't mute yourself");
                break;
            case { Permissions: var contextPerms } when contextPerms < targetPlayer.Permissions:
                await context.ReplyAsync($"You're not allowed to mute {targetPlayer.Name}");
                break;
            case not null when await playerRepository.GetPlayerNameIfNotMuted(targetPlayer.Id) == null:
                await context.ReplyAsync($"{targetPlayer.Name} is already muted");
                await context.SendAsync(new MutePlayer(targetPlayer.SessionId, true), targetPlayer.SessionId);
                break;
            case not null:
                await playerRepository.SetPlayerMuted(targetPlayer.Id, true);
                await context.SendAsync(new MutePlayer(targetPlayer.SessionId, true), targetPlayer.SessionId);
                await context.MessageAsync(targetPlayer.SessionId, "You're now muted");
                await context.ReplyAsync($"Muted {targetPlayer.Name}");
                break;
            default:
                throw new ArgumentNullException(nameof(context), "Expected command context to not be null");
        }
    }
}
