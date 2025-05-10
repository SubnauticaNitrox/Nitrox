using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal class UnmuteCommand(PlayerRepository playerRepository) : ICommandHandler<ConnectedPlayerDto>
{
    private readonly PlayerRepository playerRepository = playerRepository;

    [Description("Removes a mute from a player")]
    public async Task Execute(ICommandContext context, [Description("Player to unmute")] ConnectedPlayerDto targetPlayer)
    {
        switch (context)
        {
            case not null when context.OriginId == targetPlayer.SessionId:
                await context.ReplyAsync("You can't unmute yourself");
                break;
            case PlayerToServerCommandContext when targetPlayer.Permissions >= context.Permissions:
                await context.ReplyAsync($"You're not allowed to unmute {targetPlayer.Name}");
                break;
            case not null when await playerRepository.GetPlayerNameIfNotMuted(targetPlayer.Id) is not null:
                await context.ReplyAsync($"{targetPlayer.Name} is already unmuted");
                await context.SendAsync(new MutePlayer(targetPlayer.SessionId, false), targetPlayer.SessionId);
                break;
            case not null:
                if (!await playerRepository.SetPlayerMuted(targetPlayer.Id, false))
                {
                    await context.ReplyAsync($"Failed to unmute {targetPlayer.Name}");
                    break;
                }
                await context.SendAsync(new MutePlayer(targetPlayer.SessionId, false), targetPlayer.SessionId);
                await context.MessageAsync(targetPlayer.SessionId, "You're no longer muted");
                await context.ReplyAsync($"Unmuted {targetPlayer.Name}");
                break;
        }
    }
}
