using System.Collections;
using System.Linq;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    public class InventoryEntitySpawner : EntitySpawner<InventoryEntity>
    {

        // When we encounter a PrefabChildEntity, we need to assign the id to a prefab with the same class id and index.
        public override IEnumerator SpawnAsync(InventoryEntity entity, TaskResult<Optional<GameObject>> result)
        {
            GameObject parent = NitroxEntity.RequireObjectFrom(entity.ParentId);
            StorageContainer container = parent.GetAllComponentsInChildren<StorageContainer>()
                                               .ElementAt(entity.ComponentIndex);

            if (container)
            {
                NitroxEntity.SetNewId(container.gameObject, entity.Id);
                result.Set(Optional.OfNullable(container.gameObject));
            }
            else
            {
                Log.Error($"Unable to find prefab for: {entity}");
                result.Set(Optional.Empty);
            }

            yield break;
        }
 
        public override bool SpawnsOwnChildren(InventoryEntity entity)
        {
            return false;
        }
    }
}
