#if DEBUG
using System.Collections.Generic;
using System.ComponentModel;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.HOST)]
internal sealed class PlayerCommand(SimulationOwnershipData simulationOwnership, WorldEntityManager entityManager, PlayerManager playerManager, ILogger<PlayerCommand> logger) : ICommandHandler<Player>
{
    private readonly SimulationOwnershipData simulationOwnership = simulationOwnership;
    private readonly WorldEntityManager entityManager = entityManager;
    private readonly ILogger<PlayerCommand> logger = logger;
    private readonly PlayerManager playerManager = playerManager;

    [Description("Lists all visible cells of a player, their simulated entities per cell and the player's visible out of cell entities")]
    public Task Execute(ICommandContext context, [Description("name of the target player")] Player selectedPlayer)
    {
        if (!playerManager.TryGetPlayerBySessionId(selectedPlayer.SessionId, out Player player))
        {
            logger.ZLogError($"Player not found");
            return Task.CompletedTask;
        }

        List<AbsoluteEntityCell> visibleCells = player.GetVisibleCells();

        logger.ZLogInformation($"{player}");
        logger.ZLogInformation($"Visible cells [{visibleCells.Count}]:");
        foreach (AbsoluteEntityCell visibleCell in visibleCells)
        {
            string simulatedEntities = "";
            foreach (WorldEntity worldEntity in entityManager.GetEntities(visibleCell))
            {
                if (simulationOwnership.TryGetLock(worldEntity.Id, out SimulationOwnershipData.PlayerLock playerLock) &&
                    playerLock.Player.Id == player.Id)
                {
                    simulatedEntities += $"[{worldEntity.Id}; {worldEntity.TechType?.ToString() ?? worldEntity.ClassId}], ";
                }
            }
            logger.ZLogInformation($"{visibleCell}; {NitroxVector3.Distance(visibleCell.Position, player.Position)}");
            if (simulatedEntities.Length > 0)
            {
                // Get everything but the last ", " of the string
                logger.ZLogInformation($"{simulatedEntities[..^2]}");
            }
        }
        logger.ZLogInformation($"\nOut of cell entities:\n{string.Join(", ", player.OutOfCellVisibleEntities)}");

        return Task.CompletedTask;
    }
}
#endif
