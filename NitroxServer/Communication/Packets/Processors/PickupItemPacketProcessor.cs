using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors
{
    class PickupItemPacketProcessor : AuthenticatedPacketProcessor<PickupItem>
    {
        private readonly EntityRegistry entityRegistry;
        private readonly WorldEntityManager worldEntityManager;
        private readonly PlayerManager playerManager;
        private readonly SimulationOwnershipData simulationOwnershipData;
        private readonly PDAStateData pdaStateData;

        public PickupItemPacketProcessor(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, PlayerManager playerManager, SimulationOwnershipData simulationOwnershipData, PDAStateData pdaStateData)
        {
            this.entityRegistry = entityRegistry;
            this.worldEntityManager = worldEntityManager;
            this.playerManager = playerManager;
            this.simulationOwnershipData = simulationOwnershipData;
            this.pdaStateData = pdaStateData;
        }

        public override void Process(PickupItem packet, Player player)
        {
            if (simulationOwnershipData.RevokeOwnerOfId(packet.Id))
            {
                ushort serverId = ushort.MaxValue;
                SimulationOwnershipChange simulationOwnershipChange = new SimulationOwnershipChange(packet.Id, serverId, NitroxModel.DataStructures.SimulationLockType.TRANSIENT);
                playerManager.SendPacketToAllPlayers(simulationOwnershipChange);
            }

            // Will have the clients remove the object from their world.
            playerManager.SendPacketToOtherPlayers(packet, player);

            ConvertPickedUpWorldEntityToInventoryItemEntity(packet.Id, player);
        }

        private void ConvertPickedUpWorldEntityToInventoryItemEntity(NitroxId id, Player player)
        {
            Optional<Entity> entity = entityRegistry.GetEntityById(id);

            if (!entity.HasValue)
            {
                Log.Error($"Could not find entity {id} to see if it needed InventoryItemEntity conversion");
                return;
            }

            if (entity.Value is WorldEntity worldEntity)
            {
                // Do not track this entity in the open world anymore.
                worldEntityManager.StopTrackingEntity(worldEntity);

                // Convert the world entity into an inventory item
                InventoryItemEntity inventoryItemEntity = new InventoryItemEntity(worldEntity.Id, worldEntity.ClassId, worldEntity.TechType, worldEntity.Metadata, player.GameObjectId, worldEntity.ChildEntities);
                entityRegistry.AddOrUpdate(inventoryItemEntity);

                // Have other players respawn the item inside the inventory.
                playerManager.SendPacketToOtherPlayers(new EntitySpawnedByClient(inventoryItemEntity), player);
            }
        }
    }
}
