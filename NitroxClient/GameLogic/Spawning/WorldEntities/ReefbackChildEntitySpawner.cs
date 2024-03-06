using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class ReefbackChildEntitySpawner : IWorldEntitySpawner, IWorldEntitySyncSpawner
{
    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not ReefbackChildEntity reefbackChildEntity)
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
        if (!VerifyCanSpawnOrError(reefbackChildEntity, out ReefbackLife parentReefbackLife))
        {
            yield break;
        }

        SetupObject(reefbackChildEntity, gameObject, parentReefbackLife);

        result.Set(Optional.Of(gameObject));
    }

    public bool SpawnSync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not ReefbackChildEntity reefbackChildEntity)
        {
            return true;
        }

        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: entity.ClassId))
        {
            return false;
        }

        if (!VerifyCanSpawnOrError(reefbackChildEntity, out ReefbackLife parentReefbackLife))
        {
            return true;
        }

        GameObject gameObject = GameObjectHelper.InstantiateWithId(prefab, entity.Id);
        SetupObject(reefbackChildEntity, gameObject, parentReefbackLife);

        result.Set(gameObject);
        return true;
    }

    public bool SpawnsOwnChildren() => false;

    private static bool VerifyCanSpawnOrError(ReefbackChildEntity entity, out ReefbackLife parentReefbackLife)
    {
        if (NitroxEntity.TryGetComponentFrom(entity.ParentId, out parentReefbackLife))
        {
            return true;
        }
        Log.Error($"Could not find a valid parent with {nameof(ReefbackLife)} from Id: {entity.ParentId}");
        return false;
    }

    private static void SetupObject(ReefbackChildEntity entity, GameObject gameObject, ReefbackLife parentReefbackLife)
    {
        Transform transform = gameObject.transform;

        transform.localPosition = entity.Transform.LocalPosition.ToUnity();
        transform.localRotation = entity.Transform.LocalRotation.ToUnity();
        transform.localScale = entity.Transform.LocalScale.ToUnity();

        // Positioning from ReefbackLife.SpawnPlants and ReefbackLife.SpawnCreatures
        switch (entity.Type)
        {
            case ReefbackChildEntity.ReefbackChildType.PLANT:
                transform.SetParent(parentReefbackLife.plantSlots[0].parent, false);
                gameObject.AddComponent<ReefbackPlant>();
                break;

            case ReefbackChildEntity.ReefbackChildType.CREATURE:
                transform.SetParent(parentReefbackLife.creatureSlots[0].parent, false);
                gameObject.AddComponent<ReefbackCreature>();
                break;
        }
    }
}
