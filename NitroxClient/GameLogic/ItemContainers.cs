using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
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

        public void BroadcastItemAdd(Pickupable pickupable, GameObject owner)
        {
            string ownerGuid = GuidHelper.GetGuid(owner);
            string itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            byte[] bytes = SerializationHelper.GetBytes(pickupable.gameObject);

            ItemData itemData = new ItemData(ownerGuid, itemGuid, bytes);
            ItemContainerAdd add = new ItemContainerAdd(itemData);
            packetSender.Send(add);
        }

        public void BroadcastItemRemoval(Pickupable pickupable, GameObject owner)
        {
            string itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            string ownerGuid = GuidHelper.GetGuid(owner);

            ItemContainerRemove remove = new ItemContainerRemove(ownerGuid, itemGuid);
            packetSender.Send(remove);
        }

        public void AddItem(ItemData itemData)
        {
            Optional<GameObject> owner = GuidHelper.GetObjectFrom(itemData.ContainerGuid);

            if(owner.IsEmpty())
            {
                Log.Info("Unable to find inventory container with id: " + itemData.ContainerGuid);
                return;
            }

            Optional<ItemsContainer> opContainer = InventoryContainerHelper.GetBasedOnOwnersType(owner.Get());

            if (opContainer.IsPresent())
            {
                ItemsContainer container = opContainer.Get();
                GameObject item = SerializationHelper.GetGameObject(itemData.SerializedData);
                Pickupable pickupable = item.RequireComponent<Pickupable>();

                using (packetSender.Suppress<ItemContainerAdd>())
                {
                    container.UnsafeAdd(new InventoryItem(pickupable));
                }
            }
            else
            {
                Log.Error("Could not find container field on object " + owner.Get().name);
            }
        }

        public void RemoveItem(string ownerGuid, string itemGuid)
        {
            GameObject owner = GuidHelper.RequireObjectFrom(ownerGuid);
            GameObject item = GuidHelper.RequireObjectFrom(itemGuid);
            Optional<ItemsContainer> opContainer = InventoryContainerHelper.GetBasedOnOwnersType(owner);

            if (opContainer.IsPresent())
            {
                ItemsContainer container = opContainer.Get();
                Pickupable pickupable = item.RequireComponent<Pickupable>();

                using (packetSender.Suppress<ItemContainerRemove>())
                {
                    container.RemoveItem(pickupable, true);
                }
            }
            else
            {
                Log.Error("Could not find container field on object " + owner.name);
            }
        }
    }
}
