using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class PromoteCommand(PlayerRepository playerRepository) : ICommandHandler<ConnectedPlayerDto, Perms>
{
    private readonly PlayerRepository playerRepository = playerRepository;

    [Description("Sets specific permissions to a user")]
    public async Task Execute(ICommandContext context, [Description("The username to change the permissions of")] ConnectedPlayerDto targetPlayer, [Description("Permission level")] Perms permissions)
    {
        switch (context)
        {
            case not null when context.OriginId == targetPlayer.Id:
                await context.ReplyAsync("You can't promote yourself");
                break;
            case { Permissions: var originPerms } when originPerms < permissions:
                await context.ReplyAsync($"You're not allowed to update {targetPlayer.Name}'s permissions");
                break;
            case not null:
                if (!await playerRepository.SetPlayerPermissions(targetPlayer.Id, permissions))
                {
                    await context.ReplyAsync($"Failed to set permissions to {permissions} for player id {targetPlayer.Id}");
                    break;
                }
                await context.SendAsync(new PermsChanged(targetPlayer.Permissions), targetPlayer.SessionId);
                await context.ReplyAsync($"Updated {targetPlayer.Name}'s permissions to {permissions}");
                await context.MessageAsync(targetPlayer.SessionId, $"You've been promoted to {permissions}");
                break;
        }
    }
}
