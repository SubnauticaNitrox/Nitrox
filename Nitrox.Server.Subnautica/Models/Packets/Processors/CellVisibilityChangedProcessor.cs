using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class CellVisibilityChangedProcessor(EntitySimulation entitySimulation, WorldEntityManager worldEntityManager) : IAuthPacketProcessor<CellVisibilityChanged>
{
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, CellVisibilityChanged packet)
    {
        context.Sender.AddCells(packet.Added);
        context.Sender.RemoveCells(packet.Removed);

        List<Entity> totalEntities = new(32);
        List<SimulatedEntity> simulationChanges = new(32);

        foreach (AbsoluteEntityCell removedCell in packet.Removed)
        {
            entitySimulation.RevokeAndReassignCellEntities(context.Sender, removedCell, simulationChanges);
        }

        if (simulationChanges.Count > 0)
        {
            // so far we're only sending no longer visible entities reattribution, so no need to send it to the packet sender
            // NB: we can only reuse the same List totalSimulationChanges because the packet is immediately serialized with its data
            await context.SendToAllAsync(new SimulationOwnershipChange(simulationChanges));
        }

        // The following section is only for the sender
        foreach (AbsoluteEntityCell addedCell in packet.Added)
        {
            await worldEntityManager.LoadUnspawnedEntitiesAsync(addedCell.BatchId, false);

            simulationChanges.AddRange(entitySimulation.TryAcquireCellEntities(context.Sender, addedCell));

            worldEntityManager.FillEntitiesNonAlloc(addedCell, totalEntities);
        }

        // no need to broadcast other simulation changes because a player loading part of the world can only be given transient lock
        // on entities which aren't already simulated

        // We send this data whether it's empty or not because the client needs to know about it (see Terrain)
        await context.ReplyAsync(new SpawnEntities(totalEntities, simulationChanges, packet.Added, true));
    }
}
