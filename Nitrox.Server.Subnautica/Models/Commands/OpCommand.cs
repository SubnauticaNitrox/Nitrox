using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal class OpCommand(PlayerRepository playerRepository) : ICommandHandler<ConnectedPlayerDto>
{
    private readonly PlayerRepository playerRepository = playerRepository;

    [Description("Sets a user as admin")]
    public async Task Execute(ICommandContext context, [Description("The player to make an admin")] ConnectedPlayerDto targetPlayer)
    {
        switch (context)
        {
            case not null when targetPlayer.Permissions >= Perms.ADMIN:
                await context.ReplyAsync($"Player {targetPlayer.Name} already has {Perms.ADMIN} permissions");
                break;
            case not null:
                if (!await playerRepository.SetPlayerPermissions(targetPlayer.Id, Perms.ADMIN))
                {

                }
                await context.SendAsync(new PermsChanged(targetPlayer.Permissions), targetPlayer.SessionId); // Notify this player that they can show all the admin-related stuff
                await context.MessageAsync(targetPlayer.SessionId, $"You were promoted to {targetPlayer.Permissions}");
                await context.ReplyAsync($"Updated {targetPlayer.Name}'s permissions to {targetPlayer.Permissions}");
                break;
        }
    }
}
