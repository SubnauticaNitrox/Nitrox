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
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.HOST)]
internal sealed class PlayerCommand(SimulationOwnershipData simulationOwnership, WorldEntityManager entityManager, PlayerService playerService, ILogger<PlayerCommand> logger) : ICommandHandler<SessionId>
{
    private readonly SimulationOwnershipData simulationOwnership = simulationOwnership;
    private readonly WorldEntityManager entityManager = entityManager;
    private readonly ILogger<PlayerCommand> logger = logger;
    private readonly PlayerService playerService = playerService;

    [Description("Lists all visible cells of a player, their simulated entities per cell and the player's visible out of cell entities")]
    public Task Execute(ICommandContext context, [Description("name of the target player")] SessionId selectedPlayer)
    {
        List<AbsoluteEntityCell> visibleCells = playerService.GetProperty<VisibleCellsProperty>(selectedPlayer).GetVisibleCells();

        logger.ZLogInformation($"{selectedPlayer}");
        logger.ZLogInformation($"Visible cells [{visibleCells.Count}]:");
        foreach (AbsoluteEntityCell visibleCell in visibleCells)
        {
            string simulatedEntities = "";
            foreach (WorldEntity worldEntity in entityManager.GetEntities(visibleCell))
            {
                if (simulationOwnership.TryGetLock(worldEntity.Id, out SimulationOwnershipData.PlayerLock playerLock) &&
                    playerLock.SessionId == selectedPlayer)
                {
                    simulatedEntities += $"[{worldEntity.Id}; {worldEntity.TechType?.ToString() ?? worldEntity.ClassId}], ";
                }
            }
            logger.ZLogInformation($"{visibleCell}; {NitroxVector3.Distance(visibleCell.Position, playerService.GetProperty<PositionProperty>(selectedPlayer).Value)}");
            if (simulatedEntities.Length > 0)
            {
                // Get everything but the last ", " of the string
                logger.ZLogInformation($"{simulatedEntities[..^2]}");
            }
        }
        logger.ZLogInformation($"\nOut of cell entities:\n{string.Join(", ", playerService.GetProperty<OutOfCellVisibleEntitiesProperty>(selectedPlayer).Value)}");

        return Task.CompletedTask;
    }
}
#endif
