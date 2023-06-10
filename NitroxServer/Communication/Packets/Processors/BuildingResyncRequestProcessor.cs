using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Entities;
using System.Collections.Generic;

namespace NitroxServer.Communication.Packets.Processors;

public class BuildingResyncRequestProcessor : AuthenticatedPacketProcessor<BuildingResyncRequest>
{
    private readonly EntityRegistry entityRegistry;
    private readonly WorldEntityManager worldEntityManager;

    public BuildingResyncRequestProcessor(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager)
    {
        this.entityRegistry = entityRegistry;
        this.worldEntityManager = worldEntityManager;
    }

    public override void Process(BuildingResyncRequest packet, Player player)
    {
        Dictionary<Entity, int> entities = new();
        if (packet.ResyncEverything)
        {
            foreach (GlobalRootEntity globalRootEntity in worldEntityManager.GetGlobalRootEntities(true))
            {
                switch (globalRootEntity)
                {
                    case BuildEntity buildEntity:
                        entities.Add(buildEntity, buildEntity.OperationId);
                        break;
                    case ModuleEntity:
                        entities.Add(globalRootEntity, -1);
                        break;
                }
            }
        }
        else if (packet.EntityId != null && entityRegistry.TryGetEntityById(packet.EntityId, out Entity entity))
        {
            if (entity is BuildEntity buildEntity)
            {
                entities.Add(buildEntity, buildEntity.OperationId);
            }
            else
            {
                entities.Add(entity, -1);
            }
        }

        player.SendPacket(new BuildingResync(entities));
    }
}
