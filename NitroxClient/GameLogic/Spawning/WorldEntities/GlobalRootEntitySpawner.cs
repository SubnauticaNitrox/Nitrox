using System.Collections;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class GlobalRootEntitySpawner : SyncEntitySpawner<GlobalRootEntity>
{
    protected override IEnumerator SpawnAsync(GlobalRootEntity entity, TaskResult<Optional<GameObject>> result)
    {
        TaskResult<GameObject> gameObjectResult = new();
        yield return DefaultWorldEntitySpawner.CreateGameObject(entity.TechType.ToUnity(), entity.ClassId, entity.Id, gameObjectResult);
        GameObject gameObject = gameObjectResult.Get();

        SetupObject(entity, gameObject);

        result.Set(gameObject);
    }

    protected override bool SpawnSync(GlobalRootEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, entity.TechType.ToUnity(), entity.ClassId))
        {
            return false;
        }
        GameObject gameObject = GameObjectHelper.InstantiateWithId(prefab, entity.Id);
        SetupObject(entity, gameObject);

        result.Set(gameObject);
        return true;
    }

    private void SetupObject(GlobalRootEntity entity, GameObject gameObject)
    {
        LargeWorldEntity largeWorldEntity = gameObject.EnsureComponent<LargeWorldEntity>();
        largeWorldEntity.cellLevel = LargeWorldEntity.CellLevel.Global;
        LargeWorld.main.streamer.cellManager.RegisterEntity(largeWorldEntity);
        if (entity.ParentId != null && NitroxEntity.TryGetComponentFrom(entity.ParentId, out Transform parentTransform))
        {
            gameObject.transform.parent = parentTransform;
        }
        gameObject.transform.localPosition = entity.Transform.LocalPosition.ToUnity();
        gameObject.transform.localRotation = entity.Transform.LocalRotation.ToUnity();
        gameObject.transform.localScale = entity.Transform.LocalScale.ToUnity();

        if (gameObject.GetComponent<PlaceTool>())
        {
            PlacedWorldEntitySpawner.AdditionalSpawningSteps(gameObject);
        }
    }

    protected override bool SpawnsOwnChildren(GlobalRootEntity entity) => false;
}
