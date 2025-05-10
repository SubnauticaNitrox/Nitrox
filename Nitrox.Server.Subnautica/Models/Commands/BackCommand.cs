using System.ComponentModel;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal class BackCommand(PlayerRepository playerRepository) : ICommandHandler
{
    private readonly PlayerRepository playerRepository = playerRepository;

    [Description("Teleports you back on your last location")]
    public async Task Execute(ICommandContext context)
    {
        switch (context)
        {
            case PlayerToServerCommandContext commandContext:
                // TODO: Use more efficient query.
                PlayContext playerContext = await playerRepository.GetPlayContextById(context.OriginId);
                if (playerContext is not { Session.Player.SavedPosition: {} lastPosition })
                {
                    await context.ReplyAsync("No previous location...");
                    return;
                }

                PlayerTeleported teleportPlayer = new(playerContext.Session.Player.Id, playerContext.Position, lastPosition, playerContext.Session.Player.SavedSubRootID);
                await context.SendAsync(teleportPlayer, commandContext.OriginId);
                await context.ReplyAsync($"Teleported back to {playerContext.Session.Player.SavedPosition.Value}");
                break;
        }
    }
}
