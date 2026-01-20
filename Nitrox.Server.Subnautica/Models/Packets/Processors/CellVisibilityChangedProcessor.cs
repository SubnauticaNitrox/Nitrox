using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class CellVisibilityChangedProcessor : AuthenticatedPacketProcessor<CellVisibilityChanged>
{
    private readonly EntitySimulation entitySimulation;
    private readonly WorldEntityManager worldEntityManager;
    private readonly IPacketSender packetSender;

    public CellVisibilityChangedProcessor(IPacketSender packetSender, EntitySimulation entitySimulation, WorldEntityManager worldEntityManager)
    {
        this.packetSender = packetSender;
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
            List<WorldEntity> newEntities = worldEntityManager.GetEntities(addedCell);

            totalEntities.AddRange(newEntities);
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

        // We send this data whether or not it's empty because the client needs to know about it (see Terrain)
        packetSender.SendPacketAsync(new SpawnEntities(totalEntities, packet.Added, true), player.SessionId);
    }
}
