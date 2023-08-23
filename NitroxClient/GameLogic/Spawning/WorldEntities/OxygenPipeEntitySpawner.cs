using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class OxygenPipeEntitySpawner : EntitySpawner<OxygenPipeEntity>
{
    private readonly Entities entities;
    private readonly WorldEntitySpawner worldEntitySpawner;
    private readonly InitialPlayerSyncProcessor initialPlayerSyncProcessor;

    private readonly Dictionary<NitroxId, List<OxygenPipe>> childrenPipeEntitiesByParentId;

    public OxygenPipeEntitySpawner(Entities entities, IEntitySpawner worldEntitySpawner)
    {
        this.entities = entities;
        this.worldEntitySpawner = (WorldEntitySpawner)worldEntitySpawner;

        childrenPipeEntitiesByParentId = new();
    }

    public override IEnumerator SpawnAsync(OxygenPipeEntity entity, TaskResult<Optional<GameObject>> result)
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

        GameObject gameObject = Object.Instantiate(prefab);
        if (!IsSpawnedPrefabValid(entity, gameObject, out OxygenPipe oxygenPipe, out string errorLog))
        {
            Log.Error(errorLog);
            result.Set(Optional.Empty);
            yield break;
        }
        NitroxEntity.TryGetComponentFrom(entity.ParentPipeId, out IPipeConnection parentConnection);
        NitroxEntity.TryGetComponentFrom(entity.RootPipeId, out IPipeConnection rootConnection);
        SetupObject(entity, gameObject, oxygenPipe, parentConnection, rootConnection);

        result.Set(Optional.Of(gameObject));
    }

    public override bool SpawnSync(OxygenPipeEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: entity.ClassId))
        {
            return false;
        }

        GameObject gameObject = Object.Instantiate(prefab);
        if (!IsSpawnedPrefabValid(entity, gameObject, out OxygenPipe oxygenPipe, out string errorLog))
        {
            Log.Error(errorLog);
            return true;
        }
        NitroxEntity.TryGetComponentFrom(entity.ParentPipeId, out IPipeConnection parentConnection);
        NitroxEntity.TryGetComponentFrom(entity.RootPipeId, out IPipeConnection rootConnection);
        SetupObject(entity, gameObject, oxygenPipe, parentConnection, rootConnection);
        
        result.Set(gameObject);
        return true;
    }

    private bool IsSpawnedPrefabValid(OxygenPipeEntity entity, GameObject prefabObject, out OxygenPipe oxygenPipe, out string errorLog)
    {
        if (prefabObject.TryGetComponent(out oxygenPipe))
        {
            errorLog = string.Empty;
            return true;
        }
        errorLog = $"Couldn't find component {nameof(OxygenPipe)} on prefab with ClassId: {entity.ClassId}";
        return false;
    }

    private void SetupObject(OxygenPipeEntity entity, GameObject gameObject, OxygenPipe oxygenPipe, IPipeConnection parentConnection, IPipeConnection rootConnection)
    {
        EntityCell cellRoot = worldEntitySpawner.EnsureCell(entity);

        NitroxEntity.SetNewId(gameObject, entity.Id);
        gameObject.transform.SetParent(cellRoot.liveRoot.transform, false);
        gameObject.transform.position = entity.Transform.Position.ToUnity();
        gameObject.transform.rotation = entity.Transform.Rotation.ToUnity();
        gameObject.transform.localScale = entity.Transform.LocalScale.ToUnity();

        oxygenPipe.parentPipeUID = entity.ParentPipeId.ToString();
        oxygenPipe.rootPipeUID = entity.RootPipeId.ToString();
        oxygenPipe.parentPosition = entity.ParentPosition.ToUnity();

        // It can happen that the parent connection hasn't loaded yet (normal behaviour)
        if (parentConnection != null)
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

    public override bool SpawnsOwnChildren(OxygenPipeEntity entity)
    {
        return true;
    }
}
