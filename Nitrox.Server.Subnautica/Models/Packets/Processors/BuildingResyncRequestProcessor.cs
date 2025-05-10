using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class BuildingResyncRequestProcessor(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager) : IAuthPacketProcessor<BuildingResyncRequest>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, BuildingResyncRequest packet)
    {
        Dictionary<BuildEntity, int> buildEntities = new();
        Dictionary<ModuleEntity, int> moduleEntities = new();

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

        await context.ReplyToSender(new BuildingResync(buildEntities, moduleEntities));

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
    }
}
