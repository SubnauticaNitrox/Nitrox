using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class ModuleAddedProcessor(EntityRegistry entityRegistry, ILogger<ModuleAddedProcessor> logger) : IAuthPacketProcessor<ModuleAdded>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<ModuleAddedProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, ModuleAdded packet)
    {
        Optional<Entity> entity = entityRegistry.GetEntityById(packet.Id);

        if (!entity.HasValue)
        {
            logger.ZLogError($"Could not find entity {packet.Id} module added to a vehicle.");
            return;
        }

        if (entity.Value is InventoryItemEntity inventoryItem)
        {
            InstalledModuleEntity moduleEntity = new(packet.Slot, inventoryItem.ClassId, inventoryItem.Id, inventoryItem.TechType, inventoryItem.Metadata, packet.ParentId, inventoryItem.ChildEntities);

            // Convert the world entity into an inventory item
            entityRegistry.AddOrUpdate(moduleEntity);

            // Have other players respawn the item inside the inventory.
            await context.ReplyToOthers(new SpawnEntities(moduleEntity, forceRespawn: true));
        }
    }
}
