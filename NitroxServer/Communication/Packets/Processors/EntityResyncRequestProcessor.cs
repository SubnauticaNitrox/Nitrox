using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class EntityResyncRequestProcessor : AuthenticatedPacketProcessor<EntityResyncRequest>
{
    private readonly EntityRegistry entityRegistry;

    public EntityResyncRequestProcessor(EntityRegistry entityRegistry)
    {
        this.entityRegistry = entityRegistry;
    }

    public override void Process(EntityResyncRequest packet, Player player)
    {
        if (packet.RequestedIds.Count == 0)
        {
            return;
        }

        List<Entity> entitiesToSend = [];
        foreach (NitroxId requestedId in packet.RequestedIds)
        {
            if (entityRegistry.TryGetEntityById(requestedId, out Entity entity))
            {
                entitiesToSend.Add(entity);
            }
        }

        if (entitiesToSend.Count > 0)
        {
            Log.Debug($"Sending {entitiesToSend.Count} missing entities to {player.Name}: [{string.Join(", ", entitiesToSend.Select(e => e.Id))}]");
            player.SendPacket(new SpawnEntities(entitiesToSend, true));
        }
    }
}
