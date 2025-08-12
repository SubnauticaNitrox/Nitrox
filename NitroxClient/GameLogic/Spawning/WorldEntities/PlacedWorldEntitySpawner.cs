using System.Collections;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class PlacedWorldEntitySpawner : SyncEntitySpawner<PlacedWorldEntity>
{
    private readonly WorldEntitySpawner worldEntitySpawner;

    public PlacedWorldEntitySpawner(WorldEntitySpawner worldEntitySpawner)
    {
        this.worldEntitySpawner = worldEntitySpawner;
    }

    protected override IEnumerator SpawnAsync(PlacedWorldEntity entity, TaskResult<Optional<GameObject>> result)
    {
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
        if (!VerifyCanSpawnOrError(entity, gameObject))
        {
            yield break;
        }
        SetupObject(entity, gameObject);

        result.Set(Optional.Of(gameObject));
    }

    protected override bool SpawnSync(PlacedWorldEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: entity.ClassId))
        {
            return false;
        }

        GameObject gameObject = GameObjectHelper.InstantiateWithId(prefab, entity.Id);
        if (!VerifyCanSpawnOrError(entity, gameObject))
        {
            return true;
        }
        SetupObject(entity, gameObject);

        result.Set(gameObject);
        return true;
    }

    protected override bool SpawnsOwnChildren(PlacedWorldEntity entity) => false;

    public static void AdditionalSpawningSteps(GameObject gameObject)
    {
        if (gameObject.TryGetComponent(out PlaceTool placeTool))
        {
            if (gameObject.TryGetComponentInParent(out SubRoot subRoot))
            {
                SkyEnvironmentChanged.Send(gameObject, subRoot);
            }
            if (gameObject.TryGetComponent(out Rigidbody rigidbody))
            {
                UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, true, false);
            }
            placeTool.OnPlace();
        }
    }

    private bool VerifyCanSpawnOrError(PlacedWorldEntity entity, GameObject prefabObject)
    {
        if (prefabObject.GetComponent<PlaceTool>())
        {
            return true;
        }
        Log.Error($"Couldn't find component {nameof(PlaceTool)} on prefab with ClassId: {entity.ClassId}");
        return false;
    }

    private void SetupObject(PlacedWorldEntity entity, GameObject gameObject)
    {
        EntityCell cellRoot = worldEntitySpawner.EnsureCell(entity);

        gameObject.transform.SetParent(cellRoot.liveRoot.transform, false);
        gameObject.transform.position = entity.Transform.Position.ToUnity();
        gameObject.transform.rotation = entity.Transform.Rotation.ToUnity();
        gameObject.transform.localScale = entity.Transform.LocalScale.ToUnity();
        AdditionalSpawningSteps(gameObject);
    }
}
