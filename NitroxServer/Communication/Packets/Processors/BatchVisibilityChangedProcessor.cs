using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class BatchVisibilityChangedProcessor : AuthenticatedPacketProcessor<BatchVisibilityChanged>
{
    private readonly EntitySimulation entitySimulation;
    private readonly WorldEntityManager worldEntityManager;

    public BatchVisibilityChangedProcessor(EntitySimulation entitySimulation, WorldEntityManager worldEntityManager)
    {
        this.entitySimulation = entitySimulation;
        this.worldEntityManager = worldEntityManager;
    }

    public override void Process(BatchVisibilityChanged packet, Player player)
    {
        foreach (NitroxInt3 batchId in packet.Added)
        {
            int count = worldEntityManager.LoadUnspawnedEntities(batchId, false);

            if (count > 0)
            {
                entitySimulation.BroadcastSimulationChangesForBatchAddition(player, batchId);
            }

            List<WorldEntity> entitiesInBatch = worldEntityManager.GetEntities(batchId);

            if (entitiesInBatch.Count > 0)
            {
                SpawnEntities batchEntities = new(entitiesInBatch.Cast<Entity>().ToList());
                player.SendPacket(batchEntities);
            }
        }
    }
}
