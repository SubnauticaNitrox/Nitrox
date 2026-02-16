using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SpawnEntitiesProcessor(Entities entities, SimulationOwnership simulationOwnership, Terrain terrain) : IClientPacketProcessor<SpawnEntities>
{
    private readonly Entities entities = entities;
    private readonly SimulationOwnership simulationOwnership = simulationOwnership;
    private readonly Terrain terrain = terrain;

    public Task Process(ClientProcessorContext context, SpawnEntities packet)
    {
        if (packet.ForceRespawn)
        {
            entities.CleanupExistingEntities(packet.Entities);
        }

        if (packet.Entities.Count > 0)
        {
            foreach (SimulatedEntity simulatedEntity in packet.Simulations)
            {
                simulationOwnership.RegisterNewerSimulation(simulatedEntity.Id, simulatedEntity);
            }

            // Packet processing is done in the main thread so there's no issue calling this
            // We need a cold start so that all cleaned up entities (if force respawn is true) have time to be fully destroyed
            entities.EnqueueEntitiesToSpawn(packet.Entities, packet.SpawnedCells, packet.ForceRespawn);
            return Task.CompletedTask;
        }

        // Even if there was nothing to be spawned in the cell, we need to know about it as fully spawned
        foreach (AbsoluteEntityCell spawnedEntityCell in packet.SpawnedCells)
        {
            terrain.AddFullySpawnedCell(spawnedEntityCell);
        }
        return Task.CompletedTask;
    }
}
