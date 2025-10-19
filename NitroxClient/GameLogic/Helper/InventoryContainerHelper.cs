using System.Text.RegularExpressions;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.PlayerLogic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public class InventoryContainerHelper
    {
        private static readonly Regex LockerRegex = new(@"Locker0([0-9])StorageRoot$", RegexOptions.IgnoreCase);
        private const string LOCKER_BASE_NAME = "submarine_locker_01_0";
        private const string ESCAPEPOD_OBJECT_NAME = "EscapePod";

        /// <summary>
        ///     Gets inventory container of game object. Returns player inventory first, then child inventories.
        /// </summary>
        public static Optional<ItemsContainer> TryGetContainerByOwner(GameObject owner)
        {
            if (owner.IsLocalPlayer())
            {
                return Optional.Of(Inventory.main.container);
            }
            RemotePlayerIdentifier remotePlayerId = owner.GetComponent<RemotePlayerIdentifier>();
            if (remotePlayerId)
            {
                return Optional.Of(remotePlayerId.RemotePlayer.Inventory);
            }
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
            // TODO: in the future maybe use a switch on the PrefabId (it's always the same structure in a prefab)
            // and then statically look for the right object because we'll know exactly which one it is

            // To treat the WaterPark in parent case, we need its case to happen before the IBaseModule one because
            // IBaseModule will get the WaterPark but not get the id on the right object like in the first case
            if (parent.TryGetComponent(out WaterPark waterPark))
            {
                return waterPark.planter.TryGetIdOrWarn(out ownerId);
            }
            else if (parent.GetComponent<Constructable>() || parent.GetComponent<IBaseModule>().AliveOrNull())
            {
                return parent.TryGetIdOrWarn(out ownerId);
            }
            else if (parent.TryGetComponentInParent(out LargeRoomWaterPark largeRoomWaterPark, true) &&
                parent.TryGetNitroxId(out ownerId))
            {
                return true;
            }
            // For regular water parks, the main object contains the StorageRoot and the planter at the same level
            else if (LockerRegex.IsMatch(ownerTransform.gameObject.name))
            {
                string lockerId = ownerTransform.gameObject.name.Substring(7, 1);
                string lockerName = $"{LOCKER_BASE_NAME}{lockerId}";
                GameObject locker = parent.gameObject.FindChild(lockerName);
                if (!locker)
                {
                    Log.Error($"Could not find Locker Object: {lockerName}");
                    ownerId = null;
                    return false;
                }
                if (!locker.TryGetComponentInChildren(out StorageContainer storageContainer, true))
                {
                    Log.Error($"Could not find {nameof(StorageContainer)} From Object: {lockerName}");
                    ownerId = null;
                    return false;
                }

                return storageContainer.TryGetIdOrWarn(out ownerId);
            }
            else if (parent.name.StartsWith(ESCAPEPOD_OBJECT_NAME))
            {
                StorageContainer storageContainer = parent.RequireComponentInChildren<StorageContainer>(true);
                return storageContainer.TryGetIdOrWarn(out ownerId);
            }

            return parent.TryGetIdOrWarn(out ownerId);
        }
    }
}
