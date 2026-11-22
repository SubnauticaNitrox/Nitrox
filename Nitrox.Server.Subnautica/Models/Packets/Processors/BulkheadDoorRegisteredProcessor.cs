using System.Linq;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class BulkheadDoorRegisteredProcessor(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, ILogger<BulkheadDoorRegisteredProcessor> logger) : IAuthPacketProcessor<BulkheadDoorRegistered>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly ILogger<BulkheadDoorRegisteredProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, BulkheadDoorRegistered packet)
    {
        BulkheadDoorEntity entity = packet.Entity;
        if (entity.ParentId == null || !entityRegistry.TryGetEntityById(entity.ParentId, out BuildEntity buildEntity))
        {
            logger.ZLogError($"Trying to register a bulkhead door on a build that isn't registered (ParentId: {entity.ParentId})");
            return;
        }

        // Two players can toggle the same unregistered door at the same time, so the first registration wins:
        // the other player is told which entity to use instead of the id it generated
        BulkheadDoorEntity existing = buildEntity.ChildEntities.OfType<BulkheadDoorEntity>().FirstOrDefault(child => child.Face == entity.Face);
        if (existing != null && existing.Id != entity.Id)
        {
            await context.ReplyAsync(new BulkheadDoorRegistered(existing));
            return;
        }

        worldEntityManager.AddOrUpdateGlobalRootEntity(entity);
        await context.SendToOthersAsync(packet);
    }
}
