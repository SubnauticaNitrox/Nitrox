using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using UnityEngine;

namespace Nitrox.Client.GameLogic.Helper
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
