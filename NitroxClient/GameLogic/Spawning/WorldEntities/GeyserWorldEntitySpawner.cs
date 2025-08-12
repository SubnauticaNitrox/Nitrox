using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class GeyserWorldEntitySpawner : IWorldEntitySpawner, IWorldEntitySyncSpawner
{
    private readonly Entities entities;

    public GeyserWorldEntitySpawner(Entities entities)
    {
        this.entities = entities;
    }

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not GeyserWorldEntity geyserWorldEntity)
        {
            yield break;
        }

        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: entity.ClassId))
        {
            TaskResult<GameObject> prefabResult = new();
            yield return DefaultWorldEntitySpawner.RequestPrefab(entity.ClassId, prefabResult);
            if (!prefabResult.Get())
            {
                Log.Error($"Couldn't find a prefab for {nameof(GeyserWorldEntity)} of ClassId {entity.ClassId}");
                yield break;
            }
            prefab = prefabResult.Get();
        }

        GameObject gameObject = GameObjectHelper.InstantiateInactiveWithId(prefab, entity.Id);
        if (!VerifyCanSpawnOrError(geyserWorldEntity, gameObject, out Geyser geyser))
        {
            yield break;
        }

        SetupObject(geyserWorldEntity, cellRoot, geyser);
        gameObject.SetActive(true);

        result.Set(Optional.Of(gameObject));
    }

    public bool SpawnSync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not GeyserWorldEntity geyserWorldEntity)
        {
            return true;
        }
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: entity.ClassId))
        {
            return false;
        }

        GameObject gameObject = GameObjectHelper.InstantiateInactiveWithId(prefab, entity.Id);
        if (!VerifyCanSpawnOrError(geyserWorldEntity, gameObject, out Geyser geyser))
        {
            return true;
        }

        SetupObject(geyserWorldEntity, cellRoot, geyser);
        gameObject.SetActive(true);

        result.Set(gameObject);
        return true;
    }

    public bool SpawnsOwnChildren() => false;

    private static bool VerifyCanSpawnOrError(GeyserWorldEntity geyserEntity, GameObject prefabObject, out Geyser geyser)
    {
        if (prefabObject.TryGetComponent(out geyser))
        {
            return true;
        }

        Log.Error($"Could not find component {nameof(Geyser)} on prefab with ClassId: {geyserEntity.ClassId}");
        return false;
    }

    private static void SetupObject(GeyserWorldEntity geyserEntity, EntityCell cellRoot, Geyser geyser)
    {
        Transform transform = geyser.transform;
        transform.localPosition = geyserEntity.Transform.LocalPosition.ToUnity();
        transform.localRotation = geyserEntity.Transform.LocalRotation.ToUnity();
        transform.localScale = geyserEntity.Transform.LocalScale.ToUnity();
        transform.SetParent(cellRoot.liveRoot.transform);

        // To ensure Geyser.Start() happens first, we delay our code by a frame
        CoroutineHost.StartCoroutine(DelayedGeyserSetup(geyserEntity, geyser));
    }

    private static IEnumerator DelayedGeyserSetup(GeyserWorldEntity geyserEntity, Geyser geyser)
    {
        // Delay by an entire frame
        yield return null;
        geyser.gameObject.EnsureComponent<NitroxGeyser>().Initialize(geyserEntity, geyser);
    }
}
