using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class PickupItemPacketProcessor : AuthenticatedPacketProcessor<PickupItem>
{
    private readonly EntityRegistry entityRegistry;
    private readonly WorldEntityManager worldEntityManager;
    private readonly PlayerManager playerManager;
    private readonly SimulationOwnershipData simulationOwnershipData;

    public PickupItemPacketProcessor(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, PlayerManager playerManager, SimulationOwnershipData simulationOwnershipData)
    {
        this.entityRegistry = entityRegistry;
        this.worldEntityManager = worldEntityManager;
        this.playerManager = playerManager;
        this.simulationOwnershipData = simulationOwnershipData;
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

        ConvertPickedUpEntityToInventoryItemEntity(packet.Id, player);
    }

    private void ConvertPickedUpEntityToInventoryItemEntity(NitroxId id, Player player)
    {
        Optional<Entity> opEntity = entityRegistry.GetEntityById(id);

        if (!opEntity.HasValue)
        {
            Log.Error($"Could not find entity {id} to see if it needed InventoryItemEntity conversion");
            return;
        }

        Entity entity = opEntity.Value;
        string classId = null;

        if (entity is WorldEntity worldEntity)
        {
            // Do not track this entity in the open world anymore.
            worldEntityManager.StopTrackingEntity(worldEntity);

            classId = worldEntity.ClassId;
        }

        // Convert the entity into an inventory item entity
        InventoryItemEntity inventoryItemEntity = new(entity.Id, classId, entity.TechType, entity.Metadata, player.GameObjectId, entity.ChildEntities);
        entityRegistry.AddOrUpdate(inventoryItemEntity);

        // Have other players respawn the item inside the inventory.
        playerManager.SendPacketToOtherPlayers(new EntitySpawnedByClient(inventoryItemEntity), player);
    }
}
