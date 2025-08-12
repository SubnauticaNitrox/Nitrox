using System.Collections;
using System.Collections.Generic;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class OxygenPipeEntitySpawner : SyncEntitySpawner<OxygenPipeEntity>
{
    private readonly Entities entities;
    private readonly WorldEntitySpawner worldEntitySpawner;

    private readonly Dictionary<NitroxId, List<OxygenPipe>> childrenPipeEntitiesByParentId = new();

    public OxygenPipeEntitySpawner(Entities entities, WorldEntitySpawner worldEntitySpawner)
    {
        this.entities = entities;
        this.worldEntitySpawner = worldEntitySpawner;
    }

    protected override IEnumerator SpawnAsync(OxygenPipeEntity entity, TaskResult<Optional<GameObject>> result)
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

        GameObject gameObject = GameObjectHelper.InstantiateInactiveWithId(prefab, entity.Id);
        if (!VerifyCanSpawnOrError(entity, gameObject, out OxygenPipe oxygenPipe))
        {
            yield break;
        }

        SetupObject(entity, gameObject, oxygenPipe);
        gameObject.SetActive(true);

        result.Set(Optional.Of(gameObject));
    }

    protected override bool SpawnSync(OxygenPipeEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: entity.ClassId))
        {
            return false;
        }

        GameObject gameObject = GameObjectHelper.InstantiateInactiveWithId(prefab, entity.Id);
        if (!VerifyCanSpawnOrError(entity, gameObject, out OxygenPipe oxygenPipe))
        {
            return true;
        }

        SetupObject(entity, gameObject, oxygenPipe);
        gameObject.SetActive(true);

        result.Set(gameObject);
        return true;
    }

    protected override bool SpawnsOwnChildren(OxygenPipeEntity entity) => false;

    private bool VerifyCanSpawnOrError(OxygenPipeEntity entity, GameObject prefabObject, out OxygenPipe oxygenPipe)
    {
        if (prefabObject.TryGetComponent(out oxygenPipe))
        {
            return true;
        }
        Log.Error($"Couldn't find component {nameof(OxygenPipe)} on prefab with ClassId: {entity.ClassId}");
        return false;
    }

    private void SetupObject(OxygenPipeEntity entity, GameObject gameObject, OxygenPipe oxygenPipe)
    {
        EntityCell cellRoot = worldEntitySpawner.EnsureCell(entity);

        gameObject.transform.SetParent(cellRoot.liveRoot.transform, false);
        gameObject.transform.position = entity.Transform.Position.ToUnity();
        gameObject.transform.rotation = entity.Transform.Rotation.ToUnity();
        gameObject.transform.localScale = entity.Transform.LocalScale.ToUnity();

        // The reference IDs must be set even if the target is not spawned yet
        oxygenPipe.parentPipeUID = entity.ParentPipeId.ToString();
        oxygenPipe.rootPipeUID = entity.RootPipeId.ToString();
        oxygenPipe.parentPosition = entity.ParentPosition.ToUnity();

        // It can happen that the parent connection hasn't loaded yet (normal behaviour)
        if (NitroxEntity.TryGetComponentFrom(entity.ParentPipeId, out IPipeConnection parentConnection))
        {
            oxygenPipe.parentPosition = parentConnection.GetAttachPoint();
            parentConnection.AddChild(oxygenPipe);
        }
        else
        {
            // We add this pipe to a pending list so that its parent pipe will know which children are already spawned when being spanwed
            if (!childrenPipeEntitiesByParentId.TryGetValue(entity.ParentPipeId, out List<OxygenPipe> pendingChildren))
            {
                childrenPipeEntitiesByParentId[entity.ParentPipeId] = pendingChildren = new();
            }
            pendingChildren.Add(oxygenPipe);
        }

        if (childrenPipeEntitiesByParentId.TryGetValue(entity.Id, out List<OxygenPipe> children))
        {
            foreach (OxygenPipe childPipe in children)
            {
                oxygenPipe.AddChild(childPipe);
            }
            childrenPipeEntitiesByParentId.Remove(entity.Id);
        }

        UWE.Utils.SetIsKinematicAndUpdateInterpolation(oxygenPipe.rigidBody, true, false);
        oxygenPipe.UpdatePipe();
    }
}
