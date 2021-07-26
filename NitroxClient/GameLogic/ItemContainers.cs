using System;
using System.Text.RegularExpressions;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
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

        public void BroadcastItemAdd(Pickupable pickupable, Transform containerTransform)
        {
            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
            byte[] bytes = SerializationHelper.GetBytes(pickupable.gameObject);

            ItemData itemData = new ItemData(GetOwner(containerTransform), itemId, bytes);
            if (packetSender.Send(new ItemContainerAdd(itemData)))
            {
                Log.Debug($"Sent: Added item '{pickupable.GetTechType()}' to container '{containerTransform.gameObject.GetHierarchyPath()}'");
            }
        }

        public void BroadcastItemRemoval(Pickupable pickupable, Transform containerTransform)
        {
            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
            if (packetSender.Send(new ItemContainerRemove(GetOwner(containerTransform), itemId)))
            {
                Log.Debug($"Sent: removed item '{pickupable.GetTechType()}' from container '{containerTransform.gameObject.GetHierarchyPath()}'");
            }
        }

        public void AddItem(GameObject item, NitroxId containerId)
        {
            Optional<GameObject> owner = NitroxEntity.GetObjectFrom(containerId);
            if (!owner.HasValue)
            {
                Log.Error("Unable to find inventory container with id: " + containerId);
                return;
            }
            Optional<ItemsContainer> opContainer = InventoryContainerHelper.GetBasedOnOwnersType(owner.Value);
            if (!opContainer.HasValue)
            {
                Log.Error("Could not find container field on object " + owner.Value.name);
                return;
            }

            ItemsContainer container = opContainer.Value;
            Pickupable pickupable = item.RequireComponent<Pickupable>();
            using (packetSender.Suppress<ItemContainerAdd>())
            {
                container.UnsafeAdd(new InventoryItem(pickupable));
            }
        }

        public void RemoveItem(NitroxId ownerId, NitroxId itemId)
        {
            GameObject owner = NitroxEntity.RequireObjectFrom(ownerId);
            GameObject item = NitroxEntity.RequireObjectFrom(itemId);
            Optional<ItemsContainer> opContainer = InventoryContainerHelper.GetBasedOnOwnersType(owner);
            if (!opContainer.HasValue)
            {
                Log.Error($"Could not find item container behaviour on object '{owner.name}' with Nitrox id '{ownerId}'");
                return;
            }

            ItemsContainer container = opContainer.Value;
            Pickupable pickupable = item.RequireComponent<Pickupable>();
            using (packetSender.Suppress<ItemContainerRemove>())
            {
                container.RemoveItem(pickupable, true);
            }
        }

        public NitroxId GetCyclopsLockerId(Transform ownerTransform)
        {
            string LockerId = ownerTransform.gameObject.name.Substring(7, 1);
            GameObject locker = ownerTransform.parent.gameObject.FindChild("submarine_locker_01_0" + LockerId);
            if (!locker)
            {
                throw new Exception("Could not find Locker Object: submarine_locker_01_0" + LockerId);
            }
            StorageContainer storageContainer = locker.GetComponentInChildren<StorageContainer>();
            if (!storageContainer)
            {
                throw new Exception($"Could not find {nameof(StorageContainer)} From Object: submarine_locker_01_0{LockerId}");                
            }
            
            return NitroxEntity.GetId(storageContainer.gameObject);
        }

        public NitroxId GetEscapePodStorageId(Transform ownerTransform)
        {
            StorageContainer SC = ownerTransform.parent.gameObject.RequireComponentInChildren<StorageContainer>();
            return NitroxEntity.GetId(SC.gameObject);
        }

        private NitroxId GetOwner(Transform ownerTransform)
        {
            bool isCyclopsLocker = Regex.IsMatch(ownerTransform.gameObject.name, @"Locker0([0-9])StorageRoot$", RegexOptions.IgnoreCase);
            if (isCyclopsLocker)
            {
                return GetCyclopsLockerId(ownerTransform);
            }
            bool isEscapePodStorage = ownerTransform.parent.name.StartsWith("EscapePod");
            if (isEscapePodStorage)
            {
                return GetEscapePodStorageId(ownerTransform);
            }

            return NitroxEntity.GetId(ownerTransform.parent.gameObject);
        }
    }
}
