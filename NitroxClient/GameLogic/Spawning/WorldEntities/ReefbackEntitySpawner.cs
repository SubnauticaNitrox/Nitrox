using System;
using System.Collections;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class ReefbackEntitySpawner : IWorldEntitySpawner, IWorldEntitySyncSpawner
{
    private readonly ReefbackChildEntitySpawner reefbackChildEntitySpawner;

    public ReefbackEntitySpawner(ReefbackChildEntitySpawner reefbackChildEntitySpawner)
    {
        this.reefbackChildEntitySpawner = reefbackChildEntitySpawner;
    }

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not ReefbackEntity reefbackEntity)
        {
            yield break;
        }

        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: entity.ClassId))
        {
            TaskResult<GameObject> prefabResult = new();
            yield return DefaultWorldEntitySpawner.RequestPrefab(entity.ClassId, prefabResult);
            if (!prefabResult.Get())
            {
                Log.Error($"Couldn't find a prefab for {nameof(OxygenPipeEntity)} of ClassId {entity.ClassId}");
                yield break;
            }
            prefab = prefabResult.Get();
        }

        GameObject gameObject = GameObjectHelper.InstantiateWithId(prefab, entity.Id);
        if (!VerifyCanSpawnOrError(reefbackEntity, gameObject, out ReefbackLife reefbackLife))
        {
            yield break;
        }

        SetupObject(reefbackEntity, gameObject, cellRoot, reefbackLife);

        result.Set(gameObject);
    }

    public bool SpawnSync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not ReefbackEntity reefbackEntity)
        {
            return true;
        }

        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: entity.ClassId))
        {
            return false;
        }

        GameObject gameObject = GameObjectHelper.InstantiateWithId(prefab, entity.Id);
        if (!VerifyCanSpawnOrError(reefbackEntity, gameObject, out ReefbackLife reefbackLife))
        {
            return true;
        }

        SetupObject(reefbackEntity, gameObject, cellRoot, reefbackLife);

        result.Set(gameObject);
        return true;
    }

    public bool SpawnsOwnChildren() => false;

    private static bool VerifyCanSpawnOrError(ReefbackEntity entity, GameObject prefabObject, out ReefbackLife reefbackLife)
    {
        if (prefabObject.TryGetComponent(out reefbackLife))
        {
            return true;
        }
        Log.Error($"Could not find component {nameof(ReefbackLife)} on prefab with ClassId: {entity.ClassId}");
        return false;
    }

    private static void SetupObject(ReefbackEntity entity, GameObject gameObject, EntityCell entityCell, ReefbackLife reefbackLife)
    {
        Transform transform = gameObject.transform;
        transform.localPosition = entity.Transform.Position.ToUnity();
        transform.localRotation = entity.Transform.Rotation.ToUnity();
        transform.localScale = entity.Transform.LocalScale.ToUnity();
        entityCell.EnsureRoot();
        transform.SetParent(entityCell.liveRoot.transform);

        // Replicate only the useful parts of ReefbackLife.Initialize
        reefbackLife.initialized = true;
        reefbackLife.needToRemovePlantPhysics = false;
        reefbackLife.hasCorals = gameObject.transform.localScale.x > 0.8f;

        if (reefbackLife.hasCorals && LargeWorld.main)
        {
            string biome = LargeWorld.main.GetBiome(entity.OriginalPosition.ToUnity());
            if (!string.IsNullOrEmpty(biome) && biome.StartsWith("grassyplateaus", StringComparison.OrdinalIgnoreCase))
            {
                reefbackLife.grassIndex = 0;
            }
            else
            {
                reefbackLife.grassIndex = entity.GrassIndex;
            }
        }

        // Only useful stuff from ReefbackLife.CoSpawn
        reefbackLife.corals.SetActive(reefbackLife.hasCorals);
        reefbackLife.islands.SetActive(reefbackLife.hasCorals);
        if (reefbackLife.grassIndex >= 0 && reefbackLife.grassIndex < reefbackLife.grassVariants.Length)
        {
            reefbackLife.grassVariants[reefbackLife.grassIndex].SetActive(true);
        }
    }
}
