using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class ItemContainers
{
    private readonly IPacketSender packetSender;
    private readonly EntityMetadataManager entityMetadataManager;
    private readonly Items items;
    private readonly Entities entities;

    public ItemContainers(IPacketSender packetSender, EntityMetadataManager entityMetadataManager, Items items, Entities entities)
    {
        this.packetSender = packetSender;
        this.entityMetadataManager = entityMetadataManager;
        this.items = items;
        this.entities = entities;
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
            items.MovedIntoInventory(pickupable.gameObject, ownerId);
            return;
        }

        if (!pickupable.TryGetIdOrWarn(out NitroxId itemId))
        {
            return;
        }

        // Calls from Inventory.Pickup etc. are managed by Items.PickedUp
        if (items.PickingUpCount > 0)
        {
            return;
        }

        if (!entities.IsKnownEntity(itemId))
        {
            // If the entity existed but was deleted (for example by module remove), fall back to respawning
            items.MovedIntoInventory(pickupable.gameObject, ownerId);
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

    public void BroadcastBatteryAdd(GameObject battery, EnergyMixin energyMixin, TechType techType)
    {
        if (!battery.TryGetIdOrWarn(out NitroxId id))
        {
            return;
        }

        NitroxEntity parent = energyMixin.gameObject.FindAncestor<NitroxEntity>();
        if (!parent)
        {
            Log.Warn($"Battery entity {id} is not attached to an entity");
            return;
        }

        EnergyMixin[] components = parent.gameObject.GetAllComponentsInChildren<EnergyMixin>();
        int componentIndex = 0;
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == energyMixin)
            {
                componentIndex = i;
                break;
            }
        }

        Optional<EntityMetadata> metadata = entityMetadataManager.Extract(battery);

        InstalledBatteryEntity installedBattery = new(componentIndex, id, techType.ToDto(), metadata.OrNull(), parent.Id, []);

        EntitySpawnedByClient spawnedPacket = new(installedBattery);
        packetSender.Send(spawnedPacket);
    }
}
