using System.Collections;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class GlobalRootEntitySpawner : EntitySpawner<GlobalRootEntity>
{
    // TODO: Add a sync entity spawner
    protected override IEnumerator SpawnAsync(GlobalRootEntity entity, TaskResult<Optional<GameObject>> result)
    {
        TaskResult<GameObject> gameObjectResult = new();
        yield return DefaultWorldEntitySpawner.CreateGameObject(entity.TechType.ToUnity(), entity.ClassId, gameObjectResult);
        GameObject gameObject = gameObjectResult.Get();
        NitroxEntity.SetNewId(gameObject, entity.Id);

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

        result.Set(Optional.Of(gameObject));
    }

    protected override bool SpawnsOwnChildren(GlobalRootEntity entity) => false;
}
