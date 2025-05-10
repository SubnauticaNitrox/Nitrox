#if DEBUG
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.SUPERADMIN)]
internal class PlayerCommand(GameLogic.SimulationOwnershipData simulationOwnership, GameLogic.WorldEntityManager entityManager, ILogger<PlayerCommand> logger) : ICommandHandler<ConnectedPlayerDto>
{
    private readonly GameLogic.SimulationOwnershipData simulationOwnership = simulationOwnership;
    private readonly GameLogic.WorldEntityManager entityManager = entityManager;
    private readonly ILogger<PlayerCommand> logger = logger;

    [Description("Lists all visible cells of a player, their simulated entities per cell and the player's visible out of cell entities")]
    public Task Execute(ICommandContext context, [Description("name of the target player")] ConnectedPlayerDto player)
    {
        // TODO: USE DATABASE
        // List<AbsoluteEntityCell> visibleCells = player.GetVisibleCells();
        //
        // logger.LogInformation("{Player}", player);
        // logger.LogInformation("Visible cells [{VisibleCellCount}]:", visibleCells.Count);
        // foreach (AbsoluteEntityCell visibleCell in visibleCells)
        // {
        //     string simulatedEntities = "";
        //     foreach (WorldEntity worldEntity in entityManager.GetEntities(visibleCell))
        //     {
        //         if (simulationOwnership.TryGetLock(worldEntity.Id, out GameLogic.SimulationOwnershipData.PlayerLock playerLock) &&
        //             playerLock.Player.Id == player.Id)
        //         {
        //             simulatedEntities += $"[{worldEntity.Id}; {worldEntity.TechType?.ToString() ?? worldEntity.ClassId}], ";
        //         }
        //     }
        //     logger.LogInformation("{VisibleCell}: {Distance}", visibleCell, NitroxVector3.Distance(visibleCell.Position, player.Position));
        //     if (simulatedEntities.Length > 0)
        //     {
        //         // Get everything but the last ", " of the string
        //         logger.LogInformation(simulatedEntities[..^2]);
        //     }
        // }
        // logger.LogInformation($"\nOut of cell entities:\n{string.Join(", ", player.OutOfCellVisibleEntities)}");

        return Task.CompletedTask;
    }
}
#endif
