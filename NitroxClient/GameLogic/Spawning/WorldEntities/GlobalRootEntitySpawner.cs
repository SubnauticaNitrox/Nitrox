using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class GlobalRootEntitySpawner : EntitySpawner<GlobalRootEntity>
{
    public override IEnumerator SpawnAsync(GlobalRootEntity entity, TaskResult<Optional<GameObject>> result)
    {
        TaskResult<GameObject> gameObjectResult = new();
        yield return DefaultWorldEntitySpawner.CreateGameObject(entity.TechType.ToUnity(), entity.ClassId, gameObjectResult);
        GameObject gameObject = gameObjectResult.Get();
        NitroxEntity.SetNewId(gameObject, entity.Id);

        LargeWorldEntity largeWorldEntity = gameObject.EnsureComponent<LargeWorldEntity>();
        largeWorldEntity.cellLevel = LargeWorldEntity.CellLevel.Global;
        LargeWorld.main.streamer.cellManager.RegisterEntity(largeWorldEntity);
        gameObject.transform.localPosition = entity.Transform.LocalPosition.ToUnity();
        gameObject.transform.localRotation = entity.Transform.LocalRotation.ToUnity();
        gameObject.transform.localScale = entity.Transform.LocalScale.ToUnity();

        result.Set(Optional.Of(gameObject));
    }

    public override bool SpawnsOwnChildren(GlobalRootEntity entity)
    {
        return false;
    }
}
