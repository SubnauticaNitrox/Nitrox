using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning;
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
        private readonly LocalPlayer localPlayer;

        public ItemContainers(IPacketSender packetSender, LocalPlayer localPlayer)
        {
            this.packetSender = packetSender;
            this.localPlayer = localPlayer;
        }

        public void BroadcastItemAdd(Pickupable pickupable, Transform ownerTransform)
        {
            string ownerGuid = null;

            bool isCyclopsLocker = Regex.IsMatch(ownerTransform.gameObject.name, @"Locker0([0-9])StorageRoot$", RegexOptions.IgnoreCase);
            bool isEscapePodStorage = ownerTransform.parent.name == "EscapePod";
            if (isCyclopsLocker)
            {
                ownerGuid = GetCyclopsLockerGuid(ownerTransform);
            }
            else if (isEscapePodStorage)
            {
                ownerGuid = GetEscapePodStorageGuid(ownerTransform);
            }
            else
            {
                ownerGuid = GuidHelper.GetGuid(ownerTransform.transform.parent.gameObject);
            }
            string itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            byte[] bytes = SerializationHelper.GetBytes(pickupable.gameObject);

            ItemData itemData = new ItemData(ownerGuid, itemGuid, bytes);
            ItemContainerAdd add = new ItemContainerAdd(itemData);
            packetSender.Send(add);
        }

        public void BroadcastItemRemoval(Pickupable pickupable, Transform ownerTransform)
        {
            string ownerGuid = null;

            bool isCyclopsLocker = Regex.IsMatch(ownerTransform.gameObject.name, @"Locker0([0-9])StorageRoot$", RegexOptions.IgnoreCase);
            bool isEscapePodStorage = ownerTransform.parent.name == "EscapePod";
            if (isCyclopsLocker)
            {
                ownerGuid = GetCyclopsLockerGuid(ownerTransform);
            }
            else if (isEscapePodStorage)
            {
                ownerGuid = GetEscapePodStorageGuid(ownerTransform);
            }
            else
            {
                ownerGuid = GuidHelper.GetGuid(ownerTransform.transform.parent.gameObject);
            }
            string itemGuid = GuidHelper.GetGuid(pickupable.gameObject);
            ItemContainerRemove remove = new ItemContainerRemove(ownerGuid, itemGuid);
            packetSender.Send(remove);
        }

        public void AddItem(GameObject item, string containerGuid)
        {
            Optional<GameObject> owner = GuidHelper.GetObjectFrom(containerGuid);

            if (owner.IsEmpty())
            {
                Log.Info("Unable to find inventory container with id: " + containerGuid);
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

        public string GetCyclopsLockerGuid(Transform ownerTransform)
        {
            string LockerId = ownerTransform.gameObject.name.Substring(7, 1);

            GameObject locker = ownerTransform.parent.gameObject.FindChild("submarine_locker_01_0" + LockerId);
            if (locker != null)
            {
                StorageContainer SC = locker.GetComponentInChildren<StorageContainer>();

                if (SC != null)
                {
                    return GuidHelper.GetGuid(SC.gameObject);
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

        public string GetEscapePodStorageGuid(Transform ownerTransform)
        {
            StorageContainer SC = ownerTransform.parent.gameObject.RequireComponentInChildren<StorageContainer>();
            return GuidHelper.GetGuid(SC.gameObject);
        }
    }
}
