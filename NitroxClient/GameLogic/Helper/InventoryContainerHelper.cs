using System;
using System.Text.RegularExpressions;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public class InventoryContainerHelper
    {
        private static PlayerManager playerManager;

        public static Optional<ItemsContainer> TryGetContainerByOwner(GameObject owner)
        {
            SeamothStorageContainer seamothStorageContainer = owner.GetComponent<SeamothStorageContainer>();
            if (seamothStorageContainer)
            {
                return Optional.Of(seamothStorageContainer.container);
            }
            StorageContainer storageContainer = owner.GetComponentInChildren<StorageContainer>();
            if (storageContainer)
            {
                return Optional.Of(storageContainer.container);
            }
            BaseBioReactor baseBioReactor = owner.GetComponentInChildren<BaseBioReactor>();
            if (baseBioReactor)
            {
                return Optional.Of(baseBioReactor.container);
            }
            if (owner.name == "Player")
            {
                return Optional.Of(Inventory.Get().container);
            }
            if (owner.GetComponentInChildren<PingInstance>().GetLabel().StartsWith("Player "))
            {
                if (playerManager == null)
                {
                    playerManager = NitroxServiceLocator.LocateService<PlayerManager>();
                }

                Optional<RemotePlayer> opPlayer = playerManager.FindByName(owner.name);
                if (opPlayer.HasValue)
                {
                    return Optional.Of(opPlayer.Value.Inventory);
                }
            }

            Log.Debug("Couldn't resolve container from gameObject: " + owner.name);

            return Optional.Empty;
        }


        public static NitroxId GetOwnerId(Transform ownerTransform)
        {
            if (Regex.IsMatch(ownerTransform.gameObject.name, @"Locker0([0-9])StorageRoot$", RegexOptions.IgnoreCase))
            {
                string lockerId = ownerTransform.gameObject.name.Substring(7, 1);
                GameObject locker = ownerTransform.parent.gameObject.FindChild("submarine_locker_01_0" + lockerId);
                if (!locker)
                {
                    throw new Exception("Could not find Locker Object: submarine_locker_01_0" + lockerId);
                }
                StorageContainer storageContainer = locker.GetComponentInChildren<StorageContainer>();
                if (!storageContainer)
                {
                    throw new Exception($"Could not find {nameof(StorageContainer)} From Object: submarine_locker_01_0{lockerId}");
                }

                return NitroxEntity.GetId(storageContainer.gameObject);
            }
            if (ownerTransform.parent.name.StartsWith("EscapePod"))
            {
                StorageContainer storageContainer = ownerTransform.parent.gameObject.RequireComponentInChildren<StorageContainer>();
                return NitroxEntity.GetId(storageContainer.gameObject);
            }

            return NitroxEntity.GetId(ownerTransform.parent.gameObject);
        }
    }
}
