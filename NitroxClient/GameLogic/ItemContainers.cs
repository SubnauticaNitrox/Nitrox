using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
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
            // We don't want to broadcast that event if it's from another player's inventory
            if (containerTransform.GetComponentInParent<RemotePlayerIdentifier>(true))
            {
                return;
            }

            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);

            EntityReparented reparented = new EntityReparented(itemId, InventoryContainerHelper.GetOwnerId(containerTransform));

            if (packetSender.Send(reparented))
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
    }
}
