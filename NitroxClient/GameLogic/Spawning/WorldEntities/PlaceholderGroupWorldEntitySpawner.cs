using System.Collections;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
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
                result.Set(Optional.Empty);
                yield break;
            }
            
            PrefabPlaceholdersGroup prefabPlaceholderGroup = prefabPlaceholderGroupGameObject.Value.GetComponent<PrefabPlaceholdersGroup>();

            for (int index = 0; index < placeholderGroupEntity.ChildEntities.Count; index++)
            {
                Entity child = placeholderGroupEntity.ChildEntities[index];

                if (child == null) //Entity was picked up or removed
                {
                    continue;
                }

                if (child is PrefabPlaceholderEntity placeholder)
                {
                    PrefabPlaceholder prefabPlaceholder = prefabPlaceholderGroup.prefabPlaceholders[index];

                    IPrefabRequest prefabCoroutine = PrefabDatabase.GetPrefabAsync(prefabPlaceholder.prefabClassId);
                    yield return prefabCoroutine;
                    prefabCoroutine.TryGetPrefab(out GameObject prefab);
                    GameObject gameObject = Utils.SpawnZeroedAt(prefab, prefabPlaceholder.transform, true);

                    NitroxEntity.SetNewId(gameObject, placeholder.Id);

                    Optional<EntityMetadataProcessor> metadataProcessor = EntityMetadataProcessor.FromMetaData(entity.Metadata);

                    if (metadataProcessor.HasValue)
                    {
                        metadataProcessor.Value.ProcessMetadata(gameObject, placeholder.Metadata);
                    }
                }
                else
                {
                    Log.Debug($"Unhandled child type {child}");
                    continue;
                }
            }
        }

        public bool SpawnsOwnChildren()
        {
            return true;
        }
}

}
