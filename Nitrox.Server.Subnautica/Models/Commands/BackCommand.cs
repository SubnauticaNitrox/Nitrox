using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal sealed class BackCommand(PlayerService playerService, ITeleport teleporter) : ICommandHandler
{
    private readonly PlayerService playerService = playerService;
    private readonly ITeleport teleporter = teleporter;

    [Description("Teleports you back on your last location")]
    public async Task Execute(ICommandContext context)
    {
        NitroxVector3 checkpointPosition = playerService.GetProperty<CheckpointPositionProperty>(context.OriginId).Value;
        if (checkpointPosition == new CheckpointPositionProperty().Value)
        {
            await context.ReplyAsync("No previous location...");
        }

        await teleporter.TeleportAsync(context.OriginId, checkpointPosition, playerService.GetProperty<CheckpointSubRootIdProperty>(context.OriginId).Value);
        await context.ReplyAsync($"Teleported back to {checkpointPosition}");
    }
}
