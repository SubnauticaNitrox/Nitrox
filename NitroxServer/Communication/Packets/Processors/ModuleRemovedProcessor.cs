using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class ModuleRemovedProcessor : AuthenticatedPacketProcessor<ModuleRemoved>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;

        public ModuleRemovedProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
        }

        public override void Process(ModuleRemoved packet, Player player)
        {
            Optional<Entity> entity = entityRegistry.GetEntityById(packet.Id);

            if (!entity.HasValue)
            {
                Log.Error($"Could not find entity {packet.Id} module added to a vehicle.");
                return;
            }

            if (entity.Value is InstalledModuleEntity installedModule)
            {
                InventoryItemEntity inventoryEntity = new(installedModule.Id, installedModule.ClassId, installedModule.TechType, installedModule.Metadata, packet.NewParentId, installedModule.ChildEntities);

                // Convert the world entity into an inventory item
                entityRegistry.AddOrUpdate(inventoryEntity);

                // Have other players respawn the item inside the inventory.
                playerManager.SendPacketToOtherPlayers(new SpawnEntities(inventoryEntity, forceRespawn: true), player);
            }
        }
    }
}
