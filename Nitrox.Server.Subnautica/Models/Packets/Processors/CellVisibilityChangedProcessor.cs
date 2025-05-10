using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class CellVisibilityChangedProcessor(EntitySimulation entitySimulation, WorldEntityManager worldEntityManager, PlayerRepository playerRepository) : IAuthPacketProcessor<CellVisibilityChanged>
{
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly PlayerRepository playerRepository = playerRepository;

    public async Task Process(AuthProcessorContext context, CellVisibilityChanged packet)
    {
        // TODO: USE DATABASE - change visible cells.
        // sender.AddCells(packet.Added);
        // sender.RemoveCells(packet.Removed);

        List<Entity> totalEntities = [];
        List<SimulatedEntity> totalSimulationChanges = [];

        foreach (AbsoluteEntityCell addedCell in packet.Added)
        {
            worldEntityManager.LoadUnspawnedEntities(addedCell.BatchId, false);

            // TODO: USE DATABASE
            // totalSimulationChanges.AddRange(entitySimulation.GetSimulationChangesForCell(sender, addedCell));
            totalEntities.AddRange(worldEntityManager.GetEntities(addedCell));
        }

        foreach (AbsoluteEntityCell removedCell in packet.Removed)
        {
            // TODO: USE DATABASE
            // entitySimulation.FillWithRemovedCells(sender, removedCell, totalSimulationChanges);
        }

        // Simulation update must be broadcasted before the entities are spawned
        if (totalSimulationChanges.Count > 0)
        {
            await entitySimulation.BroadcastSimulationChanges(new(totalSimulationChanges));
        }

        if (totalEntities.Count > 0)
        {
            SpawnEntities batchEntities = new(totalEntities);
            await context.ReplyToSender(batchEntities);
        }
    }
}
