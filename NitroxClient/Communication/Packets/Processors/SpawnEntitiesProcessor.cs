using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SpawnEntitiesProcessor : ClientPacketProcessor<SpawnEntities>
{
    private readonly Entities entities;
    private readonly SimulationOwnership simulationOwnership;

    public SpawnEntitiesProcessor(Entities entities, SimulationOwnership simulationOwnership)
    {
        this.entities = entities;
        this.simulationOwnership = simulationOwnership;
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
            entities.EnqueueEntitiesToSpawn(packet.Entities);
        }
    }
}
