using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal class DeopCommand(PlayerRepository playerRepository) : ICommandHandler<ConnectedPlayerDto>
{
    private const Perms DEOP_PERMS_RESULT = Perms.PLAYER;
    private readonly PlayerRepository playerRepository = playerRepository;

    [Description("Removes admin rights from user")]
    public async Task Execute(ICommandContext context, [Description("Username to remove admin rights from")] ConnectedPlayerDto targetPlayer)
    {
        switch (context)
        {
            case not null when targetPlayer.Id == context.OriginId:
                await context.ReplyAsync("You can't deop yourself!");
                break;
            case not null when targetPlayer.Permissions >= context.Permissions:
                await context.ReplyAsync($"You're not allowed to remove admin permissions of {targetPlayer.Name}");
                break;
            case not null:
                if (!await playerRepository.SetPlayerPermissions(targetPlayer.Id, DEOP_PERMS_RESULT))
                {
                    await context.ReplyAsync($"Failed to change permissions to {DEOP_PERMS_RESULT}");
                    break;
                }
                await context.SendAsync(new PermsChanged(DEOP_PERMS_RESULT), targetPlayer.SessionId); // Notify so they no longer get admin stuff on client (which would in any way stop working)
                await context.MessageAsync(targetPlayer.SessionId, $"You were demoted to {DEOP_PERMS_RESULT}");
                await context.ReplyAsync($"Updated {targetPlayer.Name}'s permissions to {DEOP_PERMS_RESULT}");
                break;
        }
    }
}
