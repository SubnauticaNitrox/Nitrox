using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class CellVisibilityChangedProcessor(EntitySimulation entitySimulation, WorldEntityManager worldEntityManager, PlayerService playerService) : IAuthPacketProcessor<CellVisibilityChanged>
{
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly PlayerService playerService = playerService;

    public async Task Process(AuthProcessorContext context, CellVisibilityChanged packet)
    {
        VisibleCellsProperty visibleCellsProperty = playerService.GetProperty<VisibleCellsProperty>(context.Sender);
        visibleCellsProperty.AddCells(packet.Added);
        visibleCellsProperty.RemoveCells(packet.Removed);

        List<Entity> totalEntities = [];
        List<SimulatedEntity> totalSimulationChanges = [];

        foreach (AbsoluteEntityCell addedCell in packet.Added)
        {
            await worldEntityManager.LoadUnspawnedEntitiesAsync(addedCell.BatchId, false);

            totalSimulationChanges.AddRange(entitySimulation.GetSimulationChangesForCell(context.Sender, addedCell));
            List<WorldEntity> newEntities = worldEntityManager.GetEntities(addedCell);

            totalEntities.AddRange(newEntities);
        }

        foreach (AbsoluteEntityCell removedCell in packet.Removed)
        {
            entitySimulation.FillWithRemovedCells(context.Sender, removedCell, totalSimulationChanges);
        }

        // Simulation update must be broadcasted before the entities are spawned
        if (totalSimulationChanges.Count > 0)
        {
            entitySimulation.BroadcastSimulationChanges(new(totalSimulationChanges));
        }

        // We send this data whether it's empty because the client needs to know about it (see Terrain)
        await context.ReplyAsync(new SpawnEntities(totalEntities, packet.Added, true));
    }
}
