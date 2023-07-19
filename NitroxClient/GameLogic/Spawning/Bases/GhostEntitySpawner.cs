using System;
using System.Collections;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Bases.Metadata;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases;

public class GhostEntitySpawner : EntitySpawner<GhostEntity>
{
    public override IEnumerator SpawnAsync(GhostEntity entity, TaskResult<Optional<GameObject>> result)
    {
        Log.Debug($"Spawning a GhostEntity: {entity.Id}");
        if (NitroxEntity.TryGetObjectFrom(entity.Id, out GameObject gameObject))
        {
            if (gameObject.TryGetComponent(out Constructable constructable))
            {
                constructable.constructedAmount = 0;
                yield return constructable.ProgressDeconstruction();
            }
            Log.Debug($"Resynced GhostEntity {entity.Id}");
            GameObject.Destroy(gameObject);
            yield return null;
        }
        Transform parent = BuildingHandler.GetParentOrGlobalRoot(entity.ParentId);
        yield return RestoreGhost(parent, entity, result);
    }

    public override bool SpawnsOwnChildren(GhostEntity entity) => true;

    public static GhostEntity From(ConstructableBase constructableBase)
    {
        GhostEntity ghost = GhostEntity.MakeEmpty();
        ModuleEntitySpawner.FillObject(ghost, constructableBase);

        if (constructableBase.moduleFace.HasValue)
        {
            ghost.BaseFace = constructableBase.moduleFace.Value.ToDto();
        }

        ghost.BaseData = BuildEntitySpawner.GetBaseData(constructableBase.model.GetComponent<Base>());
        if (constructableBase.name.Equals("BaseDeconstructable(Clone)"))
        {
            ghost.TechType = constructableBase.techType.ToDto();
        }

        if (constructableBase.TryGetComponentInChildren(out BaseGhost baseGhost, true))
        {
            ghost.Metadata = GhostMetadataApplier.GetMetadataForGhost(baseGhost);
        }

        return ghost;
    }

    public static IEnumerator RestoreGhost(Transform parent, GhostEntity ghostEntity, TaskResult<Optional<GameObject>> result = null)
    {
        Log.Debug($"Restoring ghost {ghostEntity}");

        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: ghostEntity.ClassId))
        {
            TaskResult<GameObject> prefabResult = new();
            yield return DefaultWorldEntitySpawner.RequestPrefab(ghostEntity.ClassId, prefabResult);
            if (!prefabResult.Get())
            {
                Log.Debug($"Couldn't find a prefab for ghost of ClassId {ghostEntity.ClassId}");
                yield break;
            }
            prefab = prefabResult.Get();
        }

        bool isInBase = parent.TryGetComponent(out Base @base);

        GameObject ghostObject = UnityEngine.Object.Instantiate(prefab);
        Transform ghostTransform = ghostObject.transform;
        MoveGhostToTransform(ghostEntity, ghostTransform);

        ConstructableBase constructableBase = ghostObject.GetComponent<ConstructableBase>();
        GameObject ghostModel = constructableBase.model;
        BaseGhost baseGhost = ghostModel.GetComponent<BaseGhost>();
        Base ghostBase = ghostModel.GetComponent<Base>();
        bool isBaseDeconstructable = ghostObject.name.Equals("BaseDeconstructable(Clone)");
        if (isBaseDeconstructable && ghostEntity.TechType != null)
        {
            constructableBase.techType = ghostEntity.TechType.ToUnity();
        }
        constructableBase.constructedAmount = ghostEntity.ConstructedAmount;

        baseGhost.SetupGhost();

        yield return GhostMetadataApplier.ApplyMetadataToGhost(baseGhost, ghostEntity.Metadata, @base);

        // Necessary to wait for BaseGhost.Start()
        yield return null;
        // Verify that the metadata didn't destroy the GameObject
        if (!ghostObject)
        {
            yield break;
        }

        BuildEntitySpawner.ApplyBaseData(ghostEntity.BaseData, ghostBase);
        ghostBase.OnProtoDeserialize(null);
        if (ghostBase.cellObjects != null)
        {
            Array.Clear(ghostBase.cellObjects, 0, ghostBase.cellObjects.Length);
        }
        ghostBase.FinishDeserialization();

        if (isInBase)
        {
            @base.SetPlacementGhost(baseGhost);
            baseGhost.targetBase = @base;
            @base.RegisterBaseGhost(baseGhost);
        }
        else
        {
            ghostTransform.parent = parent;
        }
        constructableBase.SetGhostVisible(false);

        Log.Debug(BuildEntitySpawner.GetBaseData(ghostBase));
        if (!isBaseDeconstructable)
        {
            baseGhost.Place();
        }

        if (isInBase)
        {
            ghostTransform.parent = parent;
            MoveGhostToTransform(ghostEntity, ghostTransform);
        }
        constructableBase.SetState(false, false);
        constructableBase.UpdateMaterial();

        if (BuildUtils.IsUnderBaseDeconstructable(baseGhost, true) && ghostEntity.Metadata is BaseDeconstructableGhostMetadata deconstructableMetadata)
        {
            baseGhost.DisableGhostModelScripts();
        }

        NitroxEntity.SetNewId(ghostObject, ghostEntity.Id);
        result?.Set(ghostObject);
    }

    public static void MoveGhostToTransform(GhostEntity ghostEntity, Transform transform)
    {
        transform.localPosition = ghostEntity.LocalPosition.ToUnity();
        transform.localRotation = ghostEntity.LocalRotation.ToUnity();
        transform.localScale = ghostEntity.LocalScale.ToUnity();
    }
}
