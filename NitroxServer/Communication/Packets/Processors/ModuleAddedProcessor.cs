using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
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
