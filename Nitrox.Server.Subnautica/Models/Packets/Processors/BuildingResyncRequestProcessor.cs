using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

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
        Dictionary<BuildEntity, int> buildEntities = new();
        Dictionary<ModuleEntity, int> moduleEntities = new();
        void AddEntityToResync(Entity entity)
        {
            switch (entity)
            {
                case BuildEntity buildEntity:
                    buildEntities.Add(buildEntity, buildEntity.OperationId);
                    break;
                case ModuleEntity moduleEntity:
                    moduleEntities.Add(moduleEntity, -1);
                    break;
            }
        }

        if (packet.ResyncEverything)
        {
            foreach (GlobalRootEntity globalRootEntity in worldEntityManager.GetGlobalRootEntities(true))
            {
                AddEntityToResync(globalRootEntity);
            }
        }
        else if (packet.EntityId != null && entityRegistry.TryGetEntityById(packet.EntityId, out Entity entity))
        {
            AddEntityToResync(entity);
        }

        player.SendPacket(new BuildingResync(buildEntities, moduleEntities));
    }
}
