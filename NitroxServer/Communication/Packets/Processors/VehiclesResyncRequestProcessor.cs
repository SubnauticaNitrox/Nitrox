using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class VehiclesResyncRequestProcessor : AuthenticatedPacketProcessor<VehiclesResyncRequest>
{
    private readonly EntityRegistry entityRegistry;
    private readonly WorldEntityManager worldEntityManager;

    public VehiclesResyncRequestProcessor(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager)
    {
        this.entityRegistry = entityRegistry;
        this.worldEntityManager = worldEntityManager;
    }

    public override void Process(VehiclesResyncRequest packet, Player player)
    {
        List<Entity> entitiesToSend = [];

        if (packet.ResyncEverything)
        {
            entitiesToSend.AddRange(worldEntityManager.GetGlobalRootEntities<VehicleWorldEntity>(true));
        }
        else if (packet.EntityId != null && entityRegistry.TryGetEntityById(packet.EntityId, out Entity entity))
        {
            entitiesToSend.Add(entity);
        }

        if (entitiesToSend.Count > 0)
        {
            Log.Debug($"sending {entitiesToSend.Count} entities");
            player.SendPacket(new SpawnEntities(entitiesToSend, true));
        }
    }
}
