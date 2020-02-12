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
        private readonly LocalPlayer localPlayer;

        public ItemContainers(IPacketSender packetSender, LocalPlayer localPlayer)
        {
            this.packetSender = packetSender;
            this.localPlayer = localPlayer;
        }

        public void BroadcastItemAdd(Pickupable pickupable, Transform ownerTransform)
        {
            NitroxId ownerId = null;
            bool isCyclopsLocker = Regex.IsMatch(ownerTransform.gameObject.name, @"Locker0([0-9])StorageRoot$", RegexOptions.IgnoreCase);
            bool isEscapePodStorage = ownerTransform.parent.name.StartsWith("EscapePod");
            if (isCyclopsLocker)
            {
                ownerId = GetCyclopsLockerId(ownerTransform);
            }
            else if (isEscapePodStorage)
            {
                ownerId = GetEscapePodStorageId(ownerTransform);
            }
            else
            {
                ownerId = NitroxEntity.GetId(ownerTransform.transform.parent.gameObject);
            }

            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
            byte[] bytes = SerializationHelper.GetBytes(pickupable.gameObject);

            ItemData itemData = new ItemData(ownerId, itemId, bytes);
            ItemContainerAdd add = new ItemContainerAdd(itemData);
            packetSender.Send(add);
        }

        public void BroadcastItemRemoval(Pickupable pickupable, Transform ownerTransform)
        {
            NitroxId ownerId = null;

            bool isCyclopsLocker = Regex.IsMatch(ownerTransform.gameObject.name, @"Locker0([0-9])StorageRoot$", RegexOptions.IgnoreCase);
            bool isEscapePodStorage = ownerTransform.parent.name.StartsWith("EscapePod");
            if (isCyclopsLocker)
            {
                ownerId = GetCyclopsLockerId(ownerTransform);
            }
            else if (isEscapePodStorage)
            {
                ownerId = GetEscapePodStorageId(ownerTransform);
            }
            else
            {
                ownerId = NitroxEntity.GetId(ownerTransform.transform.parent.gameObject);
            }

            NitroxId itemId = NitroxEntity.GetId(pickupable.gameObject);
            ItemContainerRemove remove = new ItemContainerRemove(ownerId, itemId);
            packetSender.Send(remove);
        }

        public void AddItem(GameObject item, NitroxId containerId)
        {
            Optional<GameObject> owner = NitroxEntity.GetObjectFrom(containerId);

            if (owner.IsEmpty())
            {
                Log.Info("Unable to find inventory container with id: " + containerId);
                return;
            }

            Optional<ItemsContainer> opContainer = InventoryContainerHelper.GetBasedOnOwnersType(owner.Get());

            if (opContainer.IsPresent())
            {
                ItemsContainer container = opContainer.Get();
                Pickupable pickupable = item.RequireComponent<Pickupable>();

                using (packetSender.Suppress<ItemContainerAdd>())
                {
                    container.UnsafeAdd(new InventoryItem(pickupable));
                }
            }
            else
            {
                Log2.Instance.LogMessage(NLogType.Error, "Could not find container field on object " + owner.Get().name);
            }
        }
        
        public void RemoveItem(NitroxId ownerId, NitroxId itemId)
        {
            GameObject owner = NitroxEntity.RequireObjectFrom(ownerId);
            GameObject item = NitroxEntity.RequireObjectFrom(itemId);
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
                Log2.Instance.LogMessage(NLogType.Error, "Could not find container field on object " + owner.name);
            }
        }

        public NitroxId GetCyclopsLockerId(Transform ownerTransform)
        {
            string LockerId = ownerTransform.gameObject.name.Substring(7, 1);

            GameObject locker = ownerTransform.parent.gameObject.FindChild("submarine_locker_01_0" + LockerId);
            if (locker != null)
            {
                StorageContainer SC = locker.GetComponentInChildren<StorageContainer>();

                if (SC != null)
                {
                    return NitroxEntity.GetId(SC.gameObject);
                }
                else
                {
                    throw new Exception("Could not find StorageContainer From Object: submarine_locker_01_0" + LockerId);
                }

            }
            else
            {
                throw new Exception("Could not find Locker Object: submarine_locker_01_0" + LockerId);
            }

        }

        public NitroxId GetEscapePodStorageId(Transform ownerTransform)
        {
            StorageContainer SC = ownerTransform.parent.gameObject.RequireComponentInChildren<StorageContainer>();
            return NitroxEntity.GetId(SC.gameObject);
        }
    }
}
