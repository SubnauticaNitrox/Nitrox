using System;
using System.Collections;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases;

public class GhostEntitySpawner : EntitySpawner<GhostEntity>
{
    protected override IEnumerator SpawnAsync(GhostEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (NitroxEntity.TryGetObjectFrom(entity.Id, out GameObject gameObject))
        {
            if (gameObject.TryGetComponent(out Constructable constructable))
            {
                constructable.constructedAmount = 0;
                yield return constructable.ProgressDeconstruction();
            }
            GameObject.Destroy(gameObject);
        }
        Transform parent = BuildingHandler.GetParentOrGlobalRoot(entity.ParentId);
        yield return RestoreGhost(parent, entity, result);
    }

    protected override bool SpawnsOwnChildren(GhostEntity entity) => true;

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
            ghost.Metadata = GhostMetadataRetriever.GetMetadataForGhost(baseGhost);
        }

        return ghost;
    }

    public static IEnumerator RestoreGhost(Transform parent, GhostEntity ghostEntity, TaskResult<Optional<GameObject>> result = null)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: ghostEntity.ClassId))
        {
            TaskResult<GameObject> prefabResult = new();
            yield return DefaultWorldEntitySpawner.RequestPrefab(ghostEntity.ClassId, prefabResult);
            if (!prefabResult.Get())
            {
                Log.Error($"Couldn't find a prefab for ghost of ClassId {ghostEntity.ClassId}");
                yield break;
            }
            prefab = prefabResult.Get();
        }

        // The instructions for ghost spawning are written in a VERY PRECISE order which needs to be respected even if
        // it looks like it can be optimized. Swapping some lines may break the full spawning behaviour.
        bool isInBase = parent.TryGetComponent(out Base @base);

        // Instantiating the ghost, gathering some useful references and giving it some basic data
        GameObject ghostObject = UnityEngine.Object.Instantiate(prefab);
        Transform ghostTransform = ghostObject.transform;
        ConstructableBase constructableBase = ghostObject.GetComponent<ConstructableBase>();
        GameObject ghostModel = constructableBase.model;
        BaseGhost baseGhost = ghostModel.GetComponent<BaseGhost>();
        Base ghostBase = ghostModel.GetComponent<Base>();
        bool isBaseDeconstructable = ghostObject.name.Equals("BaseDeconstructable(Clone)");

        MoveTransformToGhostEntity(ghostTransform, ghostEntity, false);
        if (isBaseDeconstructable && ghostEntity.TechType != null)
        {
            constructableBase.techType = ghostEntity.TechType.ToUnity();
        }

        // only useful instruction in Builder.CreateGhost()
        baseGhost.SetupGhost();
        // ghost's Base should then be assigned its data (from BaseGhost.UpdatePlacement)
        BuildEntitySpawner.ApplyBaseData(ghostEntity.BaseData, ghostBase);
        ghostBase.OnProtoDeserialize(null);
        if (@base)
        {
            @base.SetPlacementGhost(baseGhost);
        }
        baseGhost.targetBase = @base;

        // Little fix for cell objects being already generated (wrongly)
        if (ghostBase.cellObjects != null)
        {
            Array.Clear(ghostBase.cellObjects, 0, ghostBase.cellObjects.Length);
        }
        ghostBase.FinishDeserialization();

        // Apply the right metadata accordingly
        IEnumerator baseDeconstructableInstructions = GhostMetadataApplier.ApplyMetadataToGhost(baseGhost, ghostEntity.Metadata, @base);
        if (baseDeconstructableInstructions != null)
        {
            yield return baseDeconstructableInstructions;
        }

        // Verify that the metadata didn't destroy the GameObject (possible in GhostMetadataApplier.ApplyBaseDeconstructableMetadataTo)
        if (!ghostObject)
        {
            yield break;
        }

        // From ConstructableBase.OnProtoDeserialize()
        // NB: Very important to fix the ghost visual glitch where the renderer is wrongly placed
        constructableBase.SetGhostVisible(false);

        // The rest is from Builder.TryPlace
        if (!isBaseDeconstructable)
        {
            // Not executed by BaseDeconstructable
            baseGhost.Place();
        }

        if (isInBase)
        {
            ghostTransform.parent = parent;
            MoveTransformToGhostEntity(ghostTransform, ghostEntity);
        }

        constructableBase.SetState(false, false);
        constructableBase.constructedAmount = ghostEntity.ConstructedAmount;
        // Addition to ensure visuals appear correctly (would be called from OnGlobalEntitiesLoaded)
        yield return constructableBase.ReplaceMaterialsAsync();

        if (isBaseDeconstructable)
        {
            baseGhost.DisableGhostModelScripts();
        }

        NitroxEntity.SetNewId(ghostObject, ghostEntity.Id);
        result?.Set(ghostObject);
    }

    private static void MoveTransformToGhostEntity(Transform transform, GhostEntity ghostEntity, bool localCoordinates = true)
    {
        if (localCoordinates)
        {
            transform.localPosition = ghostEntity.Transform.LocalPosition.ToUnity();
            transform.localRotation = ghostEntity.Transform.LocalRotation.ToUnity();
            transform.localScale = ghostEntity.Transform.LocalScale.ToUnity();
        }
        else
        {
            // TODO: Once fixed, use NitroxTransform.Position and Rotation instead of locals
            // Current issue is NitroxTransform doesn't have a reparenting behaviour when deserialized on client-side
            transform.SetPositionAndRotation(ghostEntity.Transform.LocalPosition.ToUnity(), ghostEntity.Transform.LocalRotation.ToUnity());
            transform.localScale = ghostEntity.Transform.LocalScale.ToUnity();
        }
    }
}
