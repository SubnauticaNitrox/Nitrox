using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class BuildingResyncRequestProcessor(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager) : IAuthPacketProcessor<BuildingResyncRequest>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, BuildingResyncRequest packet)
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

        await context.ReplyAsync(new BuildingResync(buildEntities, moduleEntities));
    }
}
