using System.ComponentModel;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class WarpCommand(PlayerService playerService, ITeleport teleporter, ILogger<WarpCommand> logger) : ICommandHandler<SessionId>, ICommandHandler<SessionId, SessionId>
{
    private readonly PlayerService playerService = playerService;
    private readonly ITeleport teleporter = teleporter;
    private readonly ILogger<WarpCommand> logger = logger;

    [RequiresOrigin(CommandOrigin.PLAYER)]
    [Description("Teleports you to the target player")]
    public async Task Execute(ICommandContext context, [Description("Player to teleport to")] SessionId targetPlayer)
    {
        NitroxVector3 targetPosition = playerService.GetProperty<PositionProperty>(targetPlayer).Value;
        NitroxId? targetSubRootId = playerService.GetProperty<SubRootIdProperty>(targetPlayer).Value;
        await teleporter.TeleportAsync(context.OriginId, targetPosition, targetSubRootId);
        await context.ReplyAsync($"Teleported to {playerService.GetProperty<NameProperty>(targetPlayer).Value}");
        logger.ZLogDebug($"Player '{playerService.GetProperty<NameProperty>(context.OriginId).Value}' #{context.OriginId} teleported to {targetPosition} with sub root id {targetSubRootId}");
    }

    [Description("Teleports first player to the second player")]
    public async Task Execute(ICommandContext context, SessionId warpingPlayer, SessionId targetPlayer)
    {
        await teleporter.TeleportAsync(warpingPlayer, playerService.GetProperty<PositionProperty>(targetPlayer).Value, playerService.GetProperty<SubRootIdProperty>(targetPlayer).Value);
        await context.ReplyAsync($"Teleported to {playerService.GetProperty<NameProperty>(targetPlayer).Value}");
    }
}
