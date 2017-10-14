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
                return Optional<ItemsContainer>.Of(seamothStorageContainer.container);
            }

            StorageContainer storageContainer = owner.GetComponent<StorageContainer>();

            if (storageContainer != null)
            {
                return Optional<ItemsContainer>.Of(storageContainer.container);
            }

            return Optional<ItemsContainer>.Empty();
        }
    }
}
