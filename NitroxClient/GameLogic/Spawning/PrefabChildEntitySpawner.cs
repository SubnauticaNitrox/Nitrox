using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    public class PrefabChildEntitySpawner : EntitySpawner<PrefabChildEntity>
    {
        // When we first encounter a PrefabChildEntity, we simply need to assign it the right id matching the server
        public override IEnumerator SpawnAsync(PrefabChildEntity entity, TaskResult<Optional<GameObject>> result)
        {
            GameObject parent = NitroxEntity.RequireObjectFrom(entity.ParentId);

            if (parent.transform.childCount - 1 < entity.ExistingGameObjectChildIndex)
            {
                Log.Error($"Parent {parent} did not have a child at index {entity.ExistingGameObjectChildIndex}");

                result.Set(Optional.Empty);
                yield break;
            }

            GameObject gameObject = parent.transform.GetChild(entity.ExistingGameObjectChildIndex).gameObject;

            NitroxEntity.SetNewId(gameObject, entity.Id);

            result.Set(Optional.OfNullable(gameObject));
            yield break;
        }

        public override bool SpawnsOwnChildren(PrefabChildEntity entity)
        {
            return false;
        }
    }
}
