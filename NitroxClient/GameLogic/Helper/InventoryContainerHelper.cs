using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper
{
    public class InventoryContainerHelper
    {
        public static Optional<ItemsContainer> GetBasedOnOwnersType(GameObject owner)
        {
            SeamothStorageContainer seamothStorageContainer = owner.GetComponent<SeamothStorageContainer>();
            if (seamothStorageContainer != null)
            {
                return Optional.Of(seamothStorageContainer.container);
            }
            StorageContainer storageContainer = owner.GetComponentInChildren<StorageContainer>();
            if (storageContainer != null)
            {
                return Optional.Of(storageContainer.container);
            }
            BaseBioReactor baseBioReactor = owner.GetComponentInChildren<BaseBioReactor>();
            if (baseBioReactor != null)
            {
                ItemsContainer container = (ItemsContainer)baseBioReactor.ReflectionGetProperty("container");
                return Optional.Of(container);
            }
            if (owner.name == "Player")
            {
                return Optional.Of(Inventory.Get().container);
            }

            Log.Debug("Couldn't resolve container from gameObject: " + owner.name);

            return Optional.Empty;
        }
    }
}
