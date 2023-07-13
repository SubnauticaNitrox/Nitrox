using NitroxClient.GameLogic.Spawning.Bases.PostSpawners;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures;
using NitroxModel_Subnautica.DataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UWE;
using NitroxClient.GameLogic.Bases.MetadataUtils;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxClient.GameLogic.Spawning.WorldEntities;

namespace NitroxClient.GameLogic.Bases.EntityUtils;

public static class NitroxBuild
{
    public static BuildEntity From(Base targetBase)
    {
        BuildEntity buildEntity = BuildEntity.MakeEmpty();
        if (NitroxEntity.TryGetEntityFrom(targetBase.gameObject, out NitroxEntity entity))
        {
            buildEntity.Id = entity.Id;
        }

        buildEntity.LocalPosition = targetBase.transform.localPosition.ToDto();
        buildEntity.LocalRotation = targetBase.transform.localRotation.ToDto();
        buildEntity.LocalScale = targetBase.transform.localScale.ToDto();

        buildEntity.SavedBase = NitroxBase.From(targetBase);
        buildEntity.ChildEntities.AddRange(GetChildEntities(targetBase, entity.Id));

        return buildEntity;
    }

    public static List<Entity> GetChildEntities(Base targetBase, NitroxId baseId)
    {
        List<Entity> childEntities = new();
        childEntities.AddRange(SaveInteriorPieces(targetBase));
        childEntities.AddRange(SaveModules(targetBase.transform));
        childEntities.AddRange(SaveGhosts(targetBase.transform));
        if (targetBase.TryGetComponent(out MoonpoolManager nitroxMoonpool))
        {
            childEntities.AddRange(nitroxMoonpool.GetSavedMoonpools());
        }
        childEntities.AddRange(SaveMapRooms(targetBase));

        // Making sure that childEntities are correctly parented
        foreach (Entity childEntity in childEntities)
        {
            childEntity.ParentId = baseId;
        }
        return childEntities;
    }

    public static IEnumerator ApplyBaseData(BuildEntity buildEntity, Base @base, TaskResult<Optional<GameObject>> result = null)
    {
        GameObject baseObject = @base.gameObject;
        yield return buildEntity.ApplyToAsync(@base);
        baseObject.SetActive(true);
        yield return buildEntity.RestoreGhosts(@base);
        @base.OnProtoDeserialize(null);
        @base.deserializationFinished = false;
        @base.FinishDeserialization();
        result?.Set(baseObject);
    }

    public static void SetupPower(PowerSource powerSource)
    {
        // TODO: Have synced/restored power
        powerSource.SetPower(powerSource.maxPower);
    }

    /// <summary>
    /// Destroys manually ghosts, modules, interior pieces and vehicles of a base
    /// </summary>
    /// <remarks>
    /// This is the destructive way of clearing the base, if the base isn't modified consequently, IBaseModuleGeometry under the base cells may start spamming errors.
    /// </remarks>
    public static void ClearBaseChildren(Base @base)
    {
        for (int i = @base.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = @base.transform.GetChild(i);
            if (child.GetComponent<IBaseModule>() != null || child.GetComponent<Constructable>())
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
        foreach (VehicleDockingBay vehicleDockingBay in @base.GetComponentsInChildren<VehicleDockingBay>(true))
        {
            if (vehicleDockingBay.dockedVehicle)
            {
                UnityEngine.Object.Destroy(vehicleDockingBay.dockedVehicle.gameObject);
                vehicleDockingBay.SetVehicleUndocked();
            }
        }
    }

    public static IEnumerator CreateBuild(BuildEntity buildEntity, TaskResult<Optional<GameObject>> result = null)
    {
        GameObject newBase = UnityEngine.Object.Instantiate(BaseGhost._basePrefab, LargeWorldStreamer.main.globalRoot.transform, buildEntity.LocalPosition.ToUnity(), buildEntity.LocalRotation.ToUnity(), buildEntity.LocalScale.ToUnity(), false);
        if (LargeWorld.main)
        {
            LargeWorld.main.streamer.cellManager.RegisterEntity(newBase);
        }
        Base @base = newBase.GetComponent<Base>();
        if (!@base)
        {
            Log.Debug("No Base component found");
            yield break;
        }
        yield return ApplyBaseData(buildEntity, @base, result);
    }

    private static List<InteriorPieceEntity> SaveInteriorPieces(Base targetBase)
    {
        List<InteriorPieceEntity> interiorPieces = new();
        foreach (IBaseModule module in targetBase.GetComponentsInChildren<IBaseModule>(true))
        {
            // IBaseModules without a NitroxEntity are related to BaseDeconstructable and are saved with their ghost
            if (!(module as MonoBehaviour).GetComponent<NitroxEntity>())
            {
                continue;
            }
            Log.Debug($"Base module found: {module.GetType().FullName}");
            interiorPieces.Add(NitroxInteriorPiece.From(module));
        }
        return interiorPieces;
    }

    public static List<ModuleEntity> SaveModules(Transform parent)
    {
        List<ModuleEntity> moduleEntities = new();
        foreach (Transform transform in parent)
        {
            if (transform.TryGetComponent(out Constructable constructable) && constructable is not ConstructableBase)
            {
                Log.Debug($"Constructable found: {constructable.name}");
                moduleEntities.Add(NitroxModule.From(constructable));
            }
        }
        return moduleEntities;
    }

    public static List<GhostEntity> SaveGhosts(Transform parent)
    {
        List<GhostEntity> ghostEntities = new();
        foreach (Transform transform in parent)
        {
            if (transform.TryGetComponent(out Constructable constructable) && constructable is ConstructableBase constructableBase && constructable.techType != TechType.BaseMapRoom)
            {
                Log.Debug($"BaseConstructable found: {constructableBase.name}");
                ghostEntities.Add(NitroxGhost.From(constructableBase));
            }
        }
        return ghostEntities;
    }

    private static List<MapRoomEntity> SaveMapRooms(Base targetBase)
    {
        if (!NitroxEntity.TryGetIdFrom(targetBase.gameObject, out NitroxId parentId))
        {
            return new();
        }
        List<MapRoomEntity> mapRooms = new();
        foreach (MapRoomFunctionality mapRoomFunctionality in targetBase.GetComponentsInChildren<MapRoomFunctionality>(true))
        {
            if (!NitroxEntity.TryGetIdFrom(mapRoomFunctionality.gameObject, out NitroxId mapRoomId))
            {
                continue;
            }
            Log.Debug($"MapRoom found {mapRoomId}");
            mapRooms.Add(GetMapRoomEntityFrom(mapRoomFunctionality, targetBase, mapRoomId, parentId));
        }
        return mapRooms;
    }

    public static MapRoomEntity GetMapRoomEntityFrom(MapRoomFunctionality mapRoomFunctionality, Base @base, NitroxId id, NitroxId parentId)
    {
        Int3 mapRoomCell = @base.NormalizeCell(@base.WorldToGrid(mapRoomFunctionality.transform.position));
        return new(id, parentId, mapRoomCell.ToDto());
    }

    public static void ApplyTo(this BuildEntity buildEntity, Base @base)
    {
        NitroxEntity.SetNewId(@base.gameObject, buildEntity.Id);
        buildEntity.SavedBase.ApplyTo(@base);
    }

    public static IEnumerator ApplyToAsync(this BuildEntity buildEntity, Base @base)
    {
        buildEntity.ApplyTo(@base);
        yield return buildEntity.RestoreInteriorPieces();
        yield return buildEntity.RestoreModules();
    }

    public static IEnumerator RestoreInteriorPiece(InteriorPieceEntity interiorPiece, Base @base, TaskResult<Optional<GameObject>> result = null)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: interiorPiece.ClassId))
        {
            TaskResult<GameObject> prefabResult = new();
            yield return DefaultWorldEntitySpawner.RequestPrefab(interiorPiece.ClassId, prefabResult);
            if (!prefabResult.Get())
            {
                Log.Debug($"Couldn't find a prefab for interior piece of ClassId {interiorPiece.ClassId}");
                yield break;
            }
            prefab = prefabResult.Get();
        }

        Base.Face face = interiorPiece.BaseFace.ToUnity();
        face.cell += @base.GetAnchor();
        GameObject moduleObject = @base.SpawnModule(prefab, face);
        if (moduleObject)
        {
            NitroxEntity.SetNewId(moduleObject, interiorPiece.Id);
            yield return EntityPostSpawner.ApplyPostSpawner(moduleObject, interiorPiece.Id);
            EntityMetadataProcessor.ApplyMetadata(moduleObject, interiorPiece.Metadata);
            result.Set(moduleObject);
        }
    }

    public static IEnumerator RestoreInteriorPieces(this BuildEntity buildEntity)
    {
        foreach (InteriorPieceEntity interiorPiece in buildEntity.ChildEntities.OfType<InteriorPieceEntity>())
        {
            yield return Resolve<Entities>().SpawnAsync(interiorPiece);
        }
    }

    public static IEnumerator RestoreModule(Transform parent, ModuleEntity moduleEntity, TaskResult<Optional<GameObject>> result = null)
    {
        Log.Debug($"Restoring module {moduleEntity.ClassId}");
        
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: moduleEntity.ClassId))
        {
            TaskResult<GameObject> prefabResult = new();
            yield return DefaultWorldEntitySpawner.RequestPrefab(moduleEntity.ClassId, prefabResult);
            if (!prefabResult.Get())
            {
                Log.Debug($"Couldn't find a prefab for module of ClassId {moduleEntity.ClassId}");
                yield break;
            }
            prefab = prefabResult.Get();
        }
        
        GameObject moduleObject = UnityEngine.Object.Instantiate(prefab);
        Transform moduleTransform = moduleObject.transform;
        moduleTransform.parent = parent;
        moduleTransform.localPosition = moduleEntity.LocalPosition.ToUnity();
        moduleTransform.localRotation = moduleEntity.LocalRotation.ToUnity();
        moduleTransform.localScale = moduleEntity.LocalScale.ToUnity();
        ApplyModuleData(moduleEntity, moduleObject, result);
        yield return EntityPostSpawner.ApplyPostSpawner(moduleObject, moduleEntity.Id);
    }

    public static void ApplyModuleData(ModuleEntity moduleEntity, GameObject moduleObject, TaskResult<Optional<GameObject>> result = null)
    {
        Constructable constructable = moduleObject.GetComponent<Constructable>();
        constructable.SetIsInside(moduleEntity.IsInside);
        if (moduleEntity.IsInside)
        {
            SkyEnvironmentChanged.Send(moduleObject, moduleObject.GetComponentInParent<SubRoot>(true));
        }
        else
        {
            SkyEnvironmentChanged.Send(moduleObject, (Component)null);
        }
        constructable.constructedAmount = moduleEntity.ConstructedAmount;
        constructable.SetState(moduleEntity.ConstructedAmount >= 1f, false);
        constructable.UpdateMaterial();
        NitroxEntity.SetNewId(moduleObject, moduleEntity.Id);
        EntityMetadataProcessor.ApplyMetadata(moduleObject, moduleEntity.Metadata);
        result?.Set(moduleObject);
    }

    public static IEnumerator RestoreModules(IEnumerable<ModuleEntity> modules)
    {
        foreach (ModuleEntity moduleEntity in modules)
        {
            // TODO: Should probably be optimized to avoid getting over and over the parent base object
            yield return Resolve<Entities>().SpawnAsync(moduleEntity);
        }
    }

    public static IEnumerator RestoreModules(this BuildEntity buildEntity)
    {
        yield return RestoreModules(buildEntity.ChildEntities.OfType<ModuleEntity>().Where(entity => entity is not GhostEntity));
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
        ghostEntity.MoveTransform(ghostTransform);

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

        yield return NitroxGhostMetadata.ApplyMetadataToGhost(baseGhost, ghostEntity.Metadata, @base);

        // Necessary to wait for BaseGhost.Start()
        yield return null;
        // Verify that the metadata didn't destroy the GameObject
        if (!ghostObject)
        {
            yield break;
        }

        ghostEntity.SavedBase.ApplyTo(ghostBase);
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

        Log.Debug(NitroxBase.ToString(NitroxBase.From(ghostBase)));
        if (!isBaseDeconstructable)
        {
            baseGhost.Place();
        }

        if (isInBase)
        {
            ghostTransform.parent = parent;
            ghostEntity.MoveTransform(ghostTransform);
        }

        constructableBase.SetState(false, false);
        constructableBase.UpdateMaterial();

        NitroxGhostMetadata.LateApplyMetadataToGhost(baseGhost, ghostEntity.Metadata);

        NitroxEntity.SetNewId(ghostObject, ghostEntity.Id);
        result?.Set(ghostObject);
    }

    public static IEnumerator RestoreGhosts(Transform parent, IEnumerable<GhostEntity> ghosts)
    {
        foreach (GhostEntity ghostEntity in ghosts)
        {
            yield return RestoreGhost(parent, ghostEntity);
        }
    }

    public static IEnumerator RestoreGhosts(this BuildEntity buildEntity, Base @base)
    {
        yield return RestoreGhosts(@base.transform, buildEntity.ChildEntities.OfType<GhostEntity>());
    }

    public static IEnumerator RestoreMoonpools(IEnumerable<MoonpoolEntity> moonpoolEntities, Base @base)
    {
        MoonpoolManager moonpoolManager = @base.gameObject.EnsureComponent<MoonpoolManager>();
        moonpoolManager.LoadMoonpools(moonpoolEntities);
        moonpoolManager.OnPostRebuildGeometry(@base);
        yield return moonpoolManager.SpawnVehicles();
        Log.Debug($"Restored moonpools: {moonpoolManager.GetSavedMoonpools().Count}");
    }

    public static IEnumerator RestoreMapRoom(Base @base, MapRoomEntity mapRoomEntity)
    {
        Log.Debug($"Restoring MapRoom {mapRoomEntity}");
        MapRoomFunctionality mapRoomFunctionality = @base.GetMapRoomFunctionalityForCell(mapRoomEntity.Cell.ToUnity());
        if (!mapRoomFunctionality)
        {
            Log.Error($"Couldn't find MapRoomFunctionality in base for cell {mapRoomEntity.Cell}");
            yield break;
        }
        NitroxEntity.SetNewId(mapRoomFunctionality.gameObject, mapRoomEntity.Id);
    }

    public static IEnumerator RestoreMapRooms(IEnumerable<MapRoomEntity> mapRooms, Base @base)
    {
        foreach (MapRoomEntity mapRoomEntity in mapRooms)
        {
            yield return RestoreMapRoom(@base, mapRoomEntity);
        }
    }

    public static string ToString(this SavedBuild savedBuild)
    {
        StringBuilder builder = new();
        builder.AppendLine($"Build located at {savedBuild.Position}/{savedBuild.Rotation}/{savedBuild.LocalScale}");
        builder.AppendLine(savedBuild.Base.ToString());
        return builder.ToString();
    }
}
