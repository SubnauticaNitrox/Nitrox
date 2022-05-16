using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class ItemContainers
    {
        private readonly IPacketSender packetSender;

        public ItemContainers(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BroadcastItemAdd(Pickupable pickupable, Transform containerTransform)
        {
            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
            byte[] bytes = SerializationHelper.GetBytesWithoutParent(pickupable.gameObject);
            NitroxId ownerId = InventoryContainerHelper.GetOwnerId(containerTransform);

            ItemData itemData;
            Plantable plant = pickupable.GetComponent<Plantable>();
            if (plant && plant.currentPlanter)
            {
                // special case: we want to remember the time when the plant was added, so we can simulate growth
                itemData = new PlantableItemData(ownerId, itemId, bytes, DayNightCycle.main.timePassedAsDouble);
            }
            else
            {
                itemData = new ItemData(ownerId, itemId, bytes);
            }

            if (packetSender.Send(new ItemContainerAdd(itemData)))
            {
                Log.Debug($"Sent: Added item {pickupable.GetTechType()} to container {containerTransform.gameObject.GetFullHierarchyPath()}");
            }
        }

        public void BroadcastItemRemoval(Pickupable pickupable, Transform containerTransform)
        {
            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
            if (packetSender.Send(new ItemContainerRemove(InventoryContainerHelper.GetOwnerId(containerTransform), itemId)))
            {
                Log.Debug($"Sent: Removed item {pickupable.GetTechType()} from container {containerTransform.gameObject.GetFullHierarchyPath()}");
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
            using (packetSender.Suppress<ItemContainerAdd>())
            {
                container.UnsafeAdd(new InventoryItem(pickupable));
                Log.Debug($"Received: Added item {pickupable.GetTechType()} to container {owner.Value.GetFullHierarchyPath()}");
            }
        }

        public void RemoveItem(NitroxId ownerId, NitroxId itemId)
        {
            GameObject owner = NitroxEntity.RequireObjectFrom(ownerId);
            GameObject item = NitroxEntity.RequireObjectFrom(itemId);
            Optional<ItemsContainer> opContainer = InventoryContainerHelper.TryGetContainerByOwner(owner);
            if (!opContainer.HasValue)
            {
                Log.Error($"Could not find item container behaviour on object {owner.GetFullHierarchyPath()} with id {ownerId}");
                return;
            }

            ItemsContainer container = opContainer.Value;
            Pickupable pickupable = item.RequireComponent<Pickupable>();
            using (packetSender.Suppress<ItemContainerRemove>())
            {
                container.RemoveItem(pickupable, true);
                Log.Debug($"Received: Removed item {pickupable.GetTechType()} to container {owner.GetFullHierarchyPath()}");
            }
        }
    }
}
