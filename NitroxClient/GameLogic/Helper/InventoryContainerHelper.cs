using System.Text.RegularExpressions;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public class InventoryContainerHelper
    {
        public static Optional<ItemsContainer> TryGetContainerByOwner(GameObject owner)
        {
            SeamothStorageContainer seamothStorageContainer = owner.GetComponent<SeamothStorageContainer>();
            if (seamothStorageContainer)
            {
                return Optional.Of(seamothStorageContainer.container);
            }
            StorageContainer storageContainer = owner.GetComponentInChildren<StorageContainer>(true);
            if (storageContainer)
            {
                return Optional.Of(storageContainer.container);
            }
            BaseBioReactor baseBioReactor = owner.GetComponentInChildren<BaseBioReactor>(true);
            if (baseBioReactor)
            {
                return Optional.Of(baseBioReactor.container);
            }
            if (owner.name == "Player")
            {
                return Optional.Of(Inventory.Get().container);
            }
            RemotePlayerIdentifier remotePlayerId = owner.GetComponent<RemotePlayerIdentifier>();
            if (remotePlayerId)
            {
                return Optional.Of(remotePlayerId.RemotePlayer.Inventory);
            }

            return Optional.Empty;
        }


        public static bool TryGetOwnerId(Transform ownerTransform, out NitroxId ownerId)
        {

            Transform parent = ownerTransform.parent;
            if (!parent)
            {
                Log.Error("Trying to get the ownerId of a storage that doesn't have a parent");
                ownerId = null;
                return false;
            }

            if (parent.GetComponent<Constructable>() || parent.GetComponent<IBaseModule>() != null)
            {
                return parent.TryGetIdOrWarn(out ownerId);
            }
            else if (parent.TryGetComponentInParent(out LargeRoomWaterPark largeRoomWaterPark) &&
                largeRoomWaterPark.TryGetNitroxId(out ownerId))
            {
                return true;
            }
            else if (Regex.IsMatch(ownerTransform.gameObject.name, @"Locker0([0-9])StorageRoot$", RegexOptions.IgnoreCase))
            {
                string lockerId = ownerTransform.gameObject.name.Substring(7, 1);
                GameObject locker = parent.gameObject.FindChild($"submarine_locker_01_0{lockerId}");
                if (!locker)
                {
                    Log.Error($"Could not find Locker Object: submarine_locker_01_0{lockerId}");
                    ownerId = null;
                    return false;
                }
                if (!locker.TryGetComponentInChildren(out StorageContainer storageContainer, true))
                {
                    Log.Error($"Could not find {nameof(StorageContainer)} From Object: submarine_locker_01_0{lockerId}");
                    ownerId = null;
                    return false;
                }

                return storageContainer.TryGetIdOrWarn(out ownerId);
            }
            else if (parent.name.StartsWith("EscapePod"))
            {
                StorageContainer storageContainer = parent.RequireComponentInChildren<StorageContainer>(true);
                return storageContainer.TryGetIdOrWarn(out ownerId);
            }

            return parent.TryGetIdOrWarn(out ownerId);
        }
    }
}
