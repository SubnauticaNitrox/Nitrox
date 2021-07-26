using NitroxModel.DataStructures.Util;
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
            if (owner.name == "Player")
            {
                return Optional.Of(Inventory.Get().container);
            }

            return Optional.Empty;
        }
    }
}
