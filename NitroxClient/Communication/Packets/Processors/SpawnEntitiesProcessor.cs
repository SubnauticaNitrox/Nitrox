using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class SpawnEntitiesProcessor : ClientPacketProcessor<SpawnEntities>
{
    private readonly Entities entities;

    public SpawnEntitiesProcessor(Entities entities)
    {
        this.entities = entities;
    }

    public override void Process(SpawnEntities packet)
    {
        if (packet.ForceRespawn)
        {
            entities.CleanupExistingEntities(packet.Entities);
        }

        if (packet.Entities.Count > 0)
        {
            // Packet processing is done in the main thread so there's no issue calling this
            entities.EnqueueEntitiesToSpawn(packet.Entities);
        }
    }
}
