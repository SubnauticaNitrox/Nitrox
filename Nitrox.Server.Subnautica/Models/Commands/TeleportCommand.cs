using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("tp")]
[RequiresPermission(Perms.MODERATOR)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal sealed class TeleportCommand(PlayerRepository playerRepository) : ICommandHandler<int, int, int>
{
    private readonly PlayerRepository playerRepository = playerRepository;

    [Description("Teleports yourself to a specific location")]
    public async Task Execute(ICommandContext context, [Description("x coordinate")] int x, [Description("y coordinate")] int y, [Description("z coordinate")] int z)
    {
        switch (context)
        {
            case PlayerToServerCommandContext { Player: { } player }:
                NitroxVector3 position = new(x, y, z);
                PlayContext playContext = await playerRepository.GetPlayContextById(player.SessionId);
                await context.SendToAll(new PlayerTeleported(player.Id, playContext.Position, position, null));
                await context.ReplyAsync($"Teleported to {position}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(context), "Only players can teleport themselves");
        }
    }
}
