using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    class ModuleAddedProcessor : AuthenticatedPacketProcessor<ModuleAdded>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;

        public ModuleAddedProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
        }

        public override void Process(ModuleAdded packet, Player player)
        {
            Optional<Entity> entity = entityRegistry.GetEntityById(packet.Id);

            if (!entity.HasValue)
            {
                Log.Error($"Could not find entity {packet.Id} module added to a vehicle.");
                return;
            }

            if (entity.Value is InventoryItemEntity inventoryItem)
            {
                InstalledModuleEntity moduleEntity = new(packet.Slot, inventoryItem.ClassId, inventoryItem.Id, inventoryItem.TechType, inventoryItem.Metadata, packet.ParentId, inventoryItem.ChildEntities);

                // Convert the world entity into an inventory item
                entityRegistry.AddOrUpdate(moduleEntity);

                // Have other players respawn the item inside the inventory.
                playerManager.SendPacketToOtherPlayers(new SpawnEntities(moduleEntity, forceRespawn: true), player);
            }
        }
    }
}
