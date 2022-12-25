using System;
using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning.WorldEntities
{
    public class PlaceholderGroupWorldEntitySpawner : IWorldEntitySpawner
    {
        private readonly DefaultWorldEntitySpawner defaultSpawner;

        public PlaceholderGroupWorldEntitySpawner(DefaultWorldEntitySpawner defaultSpawner)
        {
            this.defaultSpawner = defaultSpawner;
        }

        public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
        {
            Log.Debug("Using custom spawner");

            TaskResult<Optional<GameObject>> prefabPlaceholderGroupTaskResult = new();
            yield return defaultSpawner.SpawnAsync(entity, parent, cellRoot, prefabPlaceholderGroupTaskResult);
            Optional<GameObject> prefabPlaceholderGroupGameObject = prefabPlaceholderGroupTaskResult.Get();
            if (!prefabPlaceholderGroupGameObject.HasValue)
            {
                result.Set(Optional.Empty);
                yield break;
            }

            
            if (entity is not PlaceholderGroupWorldEntity placeholderGroupEntity)
            {
                Log.Error("Cast didn't work");
                result.Set(Optional.Empty);
                yield break;
            }
            
            PrefabPlaceholdersGroup prefabPlaceholderGroup = prefabPlaceholderGroupGameObject.Value.GetComponent<PrefabPlaceholdersGroup>();

            for (int index = 0; index < placeholderGroupEntity.PrefabPlaceholders.Length; index++)
            {
               NitroxModel.DataStructures.GameLogic.Entities.PrefabPlaceholder placeholder = placeholderGroupEntity.PrefabPlaceholders[index];
                if (placeholder == null) //Entity was picked up or removed
                {
                    continue;
                }

                PrefabPlaceholder prefabPlaceholder = prefabPlaceholderGroup.prefabPlaceholders[index];

                IPrefabRequest prefabCoroutine = PrefabDatabase.GetPrefabAsync(prefabPlaceholder.prefabClassId);
                yield return prefabCoroutine;
                prefabCoroutine.TryGetPrefab(out GameObject prefab);
                GameObject gameObject = Utils.SpawnZeroedAt(prefab, prefabPlaceholder.transform, true);

                NitroxEntity.SetNewId(gameObject, placeholder.Id);
            }
        }

        public bool SpawnsOwnChildren()
    {
        return true;
    }
}

}
