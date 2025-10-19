using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SpawnEntitiesProcessor : ClientPacketProcessor<SpawnEntities>
{
    private readonly Entities entities;
    private readonly SimulationOwnership simulationOwnership;
    private readonly Terrain terrain;

    public SpawnEntitiesProcessor(Entities entities, SimulationOwnership simulationOwnership, Terrain terrain)
    {
        this.entities = entities;
        this.simulationOwnership = simulationOwnership;
        this.terrain = terrain;
    }

    public override void Process(SpawnEntities packet)
    {
        if (packet.ForceRespawn)
        {
            entities.CleanupExistingEntities(packet.Entities);
        }

        if (packet.Entities.Count > 0)
        {
            if (packet.Simulations != null)
            {
                foreach (SimulatedEntity simulatedEntity in packet.Simulations)
                {
                    simulationOwnership.RegisterNewerSimulation(simulatedEntity.Id, simulatedEntity);
                }
            }

            // Packet processing is done in the main thread so there's no issue calling this
            // We need a cold start so that all cleaned up entities (if force respawn is true) have time to be fully destroyed
            entities.EnqueueEntitiesToSpawn(packet.Entities, packet.SpawnedCells, packet.ForceRespawn);
            return;
        }

        // Even if there was nothing to be spawned in the cell, we need to know about it as fully spawned
        foreach (AbsoluteEntityCell spawnedEntityCell in packet.SpawnedCells)
        {
            terrain.AddFullySpawnedCell(spawnedEntityCell);
        }
    }
}
