using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class CellVisibilityChangedProcessor : AuthenticatedPacketProcessor<CellVisibilityChanged>
{
    private readonly EntitySimulation entitySimulation;
    private readonly WorldEntityManager worldEntityManager;

    public CellVisibilityChangedProcessor(EntitySimulation entitySimulation, WorldEntityManager worldEntityManager)
    {
        this.entitySimulation = entitySimulation;
        this.worldEntityManager = worldEntityManager;
    }

    public override void Process(CellVisibilityChanged packet, Player player)
    {
        player.AddCells(packet.Added);
        player.RemoveCells(packet.Removed);

        List<Entity> totalEntities = [];
        List<SimulatedEntity> totalSimulationChanges = [];

        foreach (AbsoluteEntityCell addedCell in packet.Added)
        {
            worldEntityManager.LoadUnspawnedEntities(addedCell.BatchId, false);

            totalSimulationChanges.AddRange(entitySimulation.GetSimulationChangesForCell(player, addedCell));
            totalEntities.AddRange(worldEntityManager.GetEntities(addedCell));
        }

        foreach (AbsoluteEntityCell removedCell in packet.Removed)
        {
            entitySimulation.FillWithRemovedCells(player, removedCell, totalSimulationChanges);
        }

        // Simulation update must be broadcasted before the entities are spawned
        if (totalSimulationChanges.Count > 0)
        {
            entitySimulation.BroadcastSimulationChanges(new(totalSimulationChanges));
        }

        if (totalEntities.Count > 0)
        {
            SpawnEntities batchEntities = new(totalEntities);
            player.SendPacket(batchEntities);
        }
    }
}
