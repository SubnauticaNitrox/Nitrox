using System.Collections;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class GeyserEntitySpawner(Entities entities, WorldEntitySpawner worldEntitySpawner) : SyncEntitySpawner<GeyserEntity>
{
    private readonly Entities entities = entities;
    private readonly WorldEntitySpawner worldEntitySpawner = worldEntitySpawner;

    protected override IEnumerator SpawnAsync(GeyserEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: entity.ClassId))
        {
            TaskResult<GameObject> prefabResult = new();
            yield return DefaultWorldEntitySpawner.RequestPrefab(entity.ClassId, prefabResult);
            if (!prefabResult.Get())
            {
                Log.Error($"Couldn't find a prefab for {nameof(GeyserEntity)} of ClassId {entity.ClassId}");
                yield break;
            }
            prefab = prefabResult.Get();
        }

        GameObject gameObject = GameObjectHelper.InstantiateInactiveWithId(prefab, entity.Id);
        if (!VerifyCanSpawnOrError(entity, gameObject, out Geyser geyser))
        {
            yield break;
        }

        SetupObject(entity, geyser);
        gameObject.SetActive(true);

        result.Set(Optional.Of(gameObject));
    }

    protected override bool SpawnSync(GeyserEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: entity.ClassId))
        {
            return false;
        }

        GameObject gameObject = GameObjectHelper.InstantiateInactiveWithId(prefab, entity.Id);
        if (!VerifyCanSpawnOrError(entity, gameObject, out Geyser geyser))
        {
            return true;
        }

        SetupObject(entity, geyser);
        gameObject.SetActive(true);

        result.Set(gameObject);
        return true;
    }

    protected override bool SpawnsOwnChildren(GeyserEntity entity) => false;

    private static bool VerifyCanSpawnOrError(GeyserEntity geyserEntity, GameObject prefabObject, out Geyser geyser)
    {
        if (prefabObject.TryGetComponent(out geyser))
        {
            return true;
        }

        Log.Error($"Couldn't find component {nameof(Geyser)} on prefab with ClassId: {geyserEntity.ClassId}");
        return false;
    }

    private static void SetupObject(GeyserEntity geyserEntity, Geyser geyser)
    {
        // To ensure Geyser.Start() happens first, we delay our code by a frame
        CoroutineHost.StartCoroutine(DelayedGeyserSetup(geyserEntity, geyser));
    }

    private static IEnumerator DelayedGeyserSetup(GeyserEntity geyserEntity, Geyser geyser)
    {
        // Delay by an entire frame
        yield return null;
        geyser.gameObject.EnsureComponent<NitroxGeyser>().Initialize(geyserEntity, geyser);
    }
}
