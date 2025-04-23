using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class ItemContainers
{
    private readonly IPacketSender packetSender;
    private readonly EntityMetadataManager entityMetadataManager;
    private readonly Items items;

    public ItemContainers(IPacketSender packetSender, EntityMetadataManager entityMetadataManager, Items items)
    {
        this.packetSender = packetSender;
        this.entityMetadataManager = entityMetadataManager;
        this.items = items;
    }

    public void BroadcastItemAdd(Pickupable pickupable, Transform containerTransform, ItemsContainer container)
    {
        // We don't want to broadcast that event if it's from another player's inventory
        if (containerTransform.GetComponentInParent<RemotePlayerIdentifier>(true))
        {
            return;
        }

        if (!InventoryContainerHelper.TryGetOwnerId(containerTransform, out NitroxId ownerId))
        {
            // Error logging is done in the try function
            return;
        }

        // For planters, we'll always forcefully recreate the entity to ensure there's no desync
        if (container.containerType == ItemsContainerType.LandPlants || container.containerType == ItemsContainerType.WaterPlants)
        {
            items.Planted(pickupable.gameObject, ownerId);
            return;
        }

        if (!pickupable.TryGetIdOrWarn(out NitroxId itemId))
        {
            return;
        }

        // Calls from Inventory.Pickup are managed by Items.PickedUp
        if (items.IsInventoryPickingUp)
        {
            return;
        }
        
        if (packetSender.Send(new EntityReparented(itemId, ownerId)))
        {
            Log.Debug($"Sent: Added item ({itemId}) of type {pickupable.GetTechType()} to container {containerTransform.gameObject.GetFullHierarchyPath()}");
        }
    }

    public void AddItem(GameObject item, NitroxId containerId)
    {
        Optional<GameObject> owner = NitroxEntity.GetObjectFrom(containerId);
        if (!owner.HasValue)
        {
            Log.Error($"Unable to find inventory container with id {containerId} for {item.name}");
            return;
        }
        Optional<ItemsContainer> opContainer = InventoryContainerHelper.TryGetContainerByOwner(owner.Value);
        if (!opContainer.HasValue)
        {
            Log.Error($"Could not find container field on GameObject {owner.Value.GetFullHierarchyPath()}");
            return;
        }

        ItemsContainer container = opContainer.Value;
        Pickupable pickupable = item.RequireComponent<Pickupable>();

        using (PacketSuppressor<EntityReparented>.Suppress())
        {
            container.UnsafeAdd(new InventoryItem(pickupable));
            Log.Debug($"Received: Added item {pickupable.GetTechType()} to container {owner.Value.GetFullHierarchyPath()}");
        }
    }

    public void BroadcastBatteryAdd(GameObject gameObject, GameObject parent, TechType techType)
    {
        if (!gameObject.TryGetIdOrWarn(out NitroxId id))
        {
            return;
        }
        if (!parent.TryGetIdOrWarn(out NitroxId parentId))
        {
            return;
        }

        Optional<EntityMetadata> metadata = entityMetadataManager.Extract(gameObject);

        InstalledBatteryEntity installedBattery = new(id, techType.ToDto(), metadata.OrNull(), parentId, new());

        EntitySpawnedByClient spawnedPacket = new EntitySpawnedByClient(installedBattery);
        packetSender.Send(spawnedPacket);
    }
}
