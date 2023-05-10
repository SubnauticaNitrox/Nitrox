using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxClient.GameLogic.Spawning.Bases.PostSpawners;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Bases.New;

public static class BuildManager
{
    public static bool TryGetIdentifier(BaseDeconstructable baseDeconstructable, out BuildPieceIdentifier identifier, BaseCell baseCell = null, Base.Face? baseFace = null)
    {
        // It is unimaginable to have a BaseDeconstructable that is not child of a BaseCell
        if (!baseCell && !baseDeconstructable.TryGetComponentInParent(out baseCell))
        {
            identifier = new();
            return false;
        }

        identifier = GetIdentifier(baseDeconstructable, baseCell, baseFace);
        return true;
    }

    public static BuildPieceIdentifier GetIdentifier(BaseDeconstructable baseDeconstructable, BaseCell baseCell, Base.Face? baseFace = null)
    {
        return new()
        {
            Recipe = baseDeconstructable.recipe.ToDto(),
            BaseFace = baseFace?.ToDto() ?? baseDeconstructable.face?.ToDto(),
            BaseCell = baseCell.cell.ToDto(),
            PiecePoint = baseDeconstructable.deconstructedBase.WorldToGrid(baseDeconstructable.transform.position).ToDto()
        };
    }

    public static bool TryGetGhostFace(BaseGhost baseGhost, out Base.Face face)
    {
        // Copied code from BaseAddModuleGhost.Finish() and BaseAddFaceGhost.Finish() to obtain the face at which the module was spawned
        switch (baseGhost)
        {
            case BaseAddModuleGhost moduleGhost:
                face = moduleGhost.anchoredFace.Value;
                face.cell += baseGhost.targetBase.GetAnchor();
                return true;
            case BaseAddFaceGhost faceGhost:
                if (faceGhost.anchoredFace.HasValue)
                {
                    face = faceGhost.anchoredFace.Value;
                    face.cell += faceGhost.targetBase.GetAnchor();
                    return true;
                }
                else if (BaseAddFaceGhost.FindFirstMaskedFace(faceGhost.ghostBase, out face))
                {
                    Vector3 point = faceGhost.ghostBase.GridToWorld(Int3.zero);
                    faceGhost.targetOffset = faceGhost.targetBase.WorldToGrid(point);
                    face.cell += faceGhost.targetOffset;
                    return true;
                }
                break;
            case BaseAddWaterPark waterPark:
                if (waterPark.anchoredFace.HasValue)
                {
                    face = waterPark.anchoredFace.Value;
                    face.cell += waterPark.targetBase.GetAnchor();
                    return true;
                }
                break;
            case BaseAddMapRoomGhost:
                face = new(GetMapRoomFunctionalityCell(baseGhost), 0);
                return true;
        }

        face = default;
        return false;
    }

    /// <remarks>
    /// Even if the corresponding module was found, in some cases (with WaterParks notably) we don't want to transfer the id
    /// we then return false because the gameobject may have already been marked.
    /// </remarks>
    /// <returns>
    /// Whether or not the id was successfully transferred
    /// </returns>
    public static bool TryTransferIdFromGhostToModule(BaseGhost baseGhost, NitroxId id, ConstructableBase constructableBase, out GameObject moduleObject)
    {
        Log.Debug($"TryTransferIdFromGhostToModule({baseGhost},{id})");
        // 1. Find the face of the target piece
        Base.Face? face = null;
        bool isWaterPark = baseGhost is BaseAddWaterPark;
        bool isMapRoomGhost = baseGhost is BaseAddMapRoomGhost;
        // Only four types of ghost which spawn a module
        if ((baseGhost is BaseAddFaceGhost faceGhost && faceGhost.modulePrefab) ||
            (baseGhost is BaseAddModuleGhost moduleGhost && moduleGhost.modulePrefab) ||
            isMapRoomGhost ||
            isWaterPark)
        {
            if (TryGetGhostFace(baseGhost, out Base.Face ghostFace))
            {
                face = ghostFace;
            }
            else
            {
                Log.Error($"Couldn't find the module spawned by {baseGhost}");
                moduleObject = null;
                return false;
            }
        }
        // If the ghost is under a BaseDeconstructable(Clone), it may have an associated module
        else if (IsBaseDeconstructable(constructableBase))
        {
            face = new(constructableBase.moduleFace.Value.cell + baseGhost.targetBase.GetAnchor(), constructableBase.moduleFace.Value.direction);
        }
        else
        {
            switch (constructableBase.techType)
            {
                case TechType.BaseWaterPark:
                    // Edge case that happens when a Deconstructed WaterPark is built onto another deconstructed WaterPark that has its module
                    // A new module will be created by the current Deconstructed WaterPark which is the one we'll be aiming at
                    if (!isWaterPark)
                    {
                        IBaseModuleGeometry baseModuleGeometry = constructableBase.GetComponentInChildren<IBaseModuleGeometry>(true);
                        if (baseModuleGeometry != null)
                        {
                            face = baseModuleGeometry.geometryFace;
                        }
                    }
                    break;

                case TechType.BaseMoonpool:
                    // Moonpools are a very specific case, we tweak them to work as interior pieces (while they're not)
                    Optional<GameObject> objectOptional = baseGhost.targetBase.gameObject.EnsureComponent<MoonpoolManager>().RegisterMoonpool(constructableBase.transform, id);
                    moduleObject = objectOptional.Value;
                    return moduleObject;

                case TechType.BaseMapRoom:
                    // In the case the ghost is under a BaseDeconstructable, this is a good way to identify a MapRoom
                    face = new(GetMapRoomFunctionalityCell(baseGhost), 0);
                    isMapRoomGhost = true;
                    break;

                default:
                    moduleObject = null;
                    return false;
            }
        }

        if (!face.HasValue)
        {
            if (constructableBase.techType != TechType.BaseWaterPark)
            {
                Log.Error($"No face could be found for ghost {baseGhost}");
            }
            moduleObject = null;
            return false;
        }

        // 2. Use that face to find the newly created piece and set its id to the desired one
        if (isMapRoomGhost)
        {
            MapRoomFunctionality mapRoomFunctionality = baseGhost.targetBase.GetMapRoomFunctionalityForCell(face.Value.cell);
            if (mapRoomFunctionality)
            {
                // As MapRooms can be built as the first piece of a base, we need to make sure that they receive a new id if they're not in a base
                if (constructableBase.GetComponentInParent<Base>(true))
                {
                    NitroxEntity.SetNewId(mapRoomFunctionality.gameObject, id);
                }
                else
                {
                    NitroxEntity.SetNewId(mapRoomFunctionality.gameObject, id.Increment());
                }
                moduleObject = mapRoomFunctionality.gameObject;
                return true;
            }
            Log.Error($"Couldn't find MapRoomFunctionality of built MapRoom (cell: {face.Value.cell})");
            moduleObject = null;
            return false;
        }

        IBaseModule module = baseGhost.targetBase.GetModule(face.Value);
        if (module != null)
        {
            // If the WaterPark is higher than one, it means that the newly built WaterPark will be merged with one that already has a NitroxEntity
            if (module is WaterPark waterPark && waterPark.height > 1)
            {
                Log.Debug($"Found WaterPark higher than 1 [{waterPark.height}], not transferring NitroxEntity to it");
                // as the WaterPark is necessarily merged, we won't need to do anything about it
                moduleObject = null;
                return false;
            }

            Log.Debug($"Successfully transferred NitroxEntity to {module} [{id}]");
            moduleObject = (module as Component).gameObject;
            NitroxEntity.SetNewId(moduleObject, id);
            return true;
        }
        // When a WaterPark is merged with another one, we won't find its module but we don't care about that
        else if (!isWaterPark)
        {
            Log.Error("Couldn't find the module's GameObject of built interior piece when transfering its NitroxEntity to the module.");
        }

        moduleObject = null;
        return false;
    }

    /// <remarks>
    ///     The criteria to make sure that a ConstructableBase is one of a BaseDeconstructable is if it has a moduleFace
    ///     because this field is only filled for the base deconstruction (<see cref="BaseDeconstructable.Deconstruct"/>, <seealso cref="ConstructableBase.LinkModule(Base.Face?)"/>).
    /// </remarks>
    public static bool IsBaseDeconstructable(ConstructableBase constructableBase)
    {
        return constructableBase.moduleFace.HasValue;
    }

    /// <remarks>
    /// A BaseDeconstructable's ghost component is a simple BaseGhost so we need to identify it by the parent ConstructableBase instead.
    /// </remarks>
    /// <param name="faceAlreadyLinked">Whether <see cref="ConstructableBase.moduleFace"/> was already set or not</param>
    public static bool IsUnderBaseDeconstructable(BaseGhost baseGhost, bool faceNotLinked = false)
    {
        return baseGhost.TryGetComponentInParent(out ConstructableBase constructableBase) &&
            (IsBaseDeconstructable(constructableBase) || faceNotLinked);
    }

    public static Int3 GetMapRoomFunctionalityCell(BaseGhost baseGhost)
    {
        // Code found from Base.GetMapRoomFunctionalityForCell
        return baseGhost.targetBase.NormalizeCell(baseGhost.targetBase.WorldToGrid(baseGhost.ghostBase.occupiedBounds.center));
    }
}

public static class NitroxGlobalRoot
{
    public static SavedGlobalRoot From(Transform globalRoot)
    {
        SavedGlobalRoot savedGlobalRoot = new() { Builds = new(), Modules = new(), Ghosts = new() };
        foreach (Transform child in globalRoot)
        {
            if (child.TryGetComponent(out Base @base))
            {
                savedGlobalRoot.Builds.Add(NitroxBuild.From(@base));
            }
            else if (child.TryGetComponent(out Constructable constructable))
            {
                if (constructable is ConstructableBase constructableBase)
                {
                    savedGlobalRoot.Ghosts.Add(NitroxGhost.From(constructableBase));
                    continue;
                }
                savedGlobalRoot.Modules.Add(NitroxModule.From(constructable));
            }
        }
        return savedGlobalRoot;
    }

    public static string ToString(this SavedGlobalRoot savedGlobalRoot)
    {
        return $"SavedGlobalRoot [Builds: {savedGlobalRoot.Builds.Count}, Modules: {savedGlobalRoot.Modules.Count}, Ghosts: {savedGlobalRoot.Ghosts.Count}]";
    }
}

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
        yield return buildEntity.ApplyToAsync(@base);
        newBase.SetActive(true);
        yield return buildEntity.RestoreGhosts(@base);
        @base.OnProtoDeserialize(null);
        @base.FinishDeserialization();
        result?.Set(newBase);
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
        return new (id, parentId, mapRoomCell.ToDto());
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
        IPrefabRequest request = PrefabDatabase.GetPrefabAsync(interiorPiece.ClassId);
        yield return request;
        if (!request.TryGetPrefab(out GameObject prefab))
        {
            Log.Debug($"Couldn't find a prefab for interior piece of ClassId {interiorPiece.ClassId}");
            yield break;
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
        IPrefabRequest request = PrefabDatabase.GetPrefabAsync(moduleEntity.ClassId);
        yield return request;
        if (!request.TryGetPrefab(out GameObject prefab))
        {
            Log.Debug($"Couldn't find a prefab for module of ClassId {moduleEntity.ClassId}");
            yield break;
        }
        GameObject moduleObject = GameObject.Instantiate(prefab);
        Transform moduleTransform = moduleObject.transform;
        moduleTransform.parent = parent;
        moduleTransform.localPosition = moduleEntity.LocalPosition.ToUnity();
        moduleTransform.localRotation = moduleEntity.LocalRotation.ToUnity();
        moduleTransform.localScale = moduleEntity.LocalScale.ToUnity();

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
        yield return EntityPostSpawner.ApplyPostSpawner(moduleObject, moduleEntity.Id);
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
        IPrefabRequest request = PrefabDatabase.GetPrefabAsync(ghostEntity.ClassId);
        yield return request;
        if (!request.TryGetPrefab(out GameObject prefab))
        {
            Log.Debug($"Couldn't find a prefab for module of ClassId {ghostEntity.ClassId}");
            yield break;
        }
        bool isInBase = parent.TryGetComponent(out Base @base);

        GameObject ghostObject = GameObject.Instantiate(prefab);
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

public static class NitroxBase
{
    public static SavedBase From(Base targetBase)
    {
        SavedBase savedBase = new();
        if (targetBase.baseShape != null)
        {
            savedBase.BaseShape = targetBase.baseShape.ToInt3().ToDto();
        }
        if (targetBase.faces != null)
        {
            savedBase.Faces = Array.ConvertAll(targetBase.faces, faceType => (int)faceType);
        }
        if (targetBase.cells != null)
        {
            savedBase.Cells = Array.ConvertAll(targetBase.cells, cellType => (int)cellType);
        }
        if (targetBase.links != null)
        {
            savedBase.Links = targetBase.links;
        }
        if (targetBase.cellOffset != null)
        {
            savedBase.CellOffset = targetBase.cellOffset.ToDto();
        }
        if (targetBase.masks != null)
        {
            savedBase.Masks = targetBase.masks;
        }
        if (targetBase.isGlass != null)
        {
            savedBase.IsGlass = Array.ConvertAll(targetBase.isGlass, isGlass => isGlass ? 1 : 0);
        }
        if (targetBase.anchor != null)
        {
            savedBase.Anchor = targetBase.anchor.ToDto();
        }
        return savedBase;
    }

    public static void ApplyTo(this SavedBase savedBase, Base @base)
    {
        if (savedBase.BaseShape != null)
        {
            @base.baseShape = new(savedBase.BaseShape.ToUnity());
        }
        if (savedBase.Faces != null)
        {
            @base.faces = Array.ConvertAll(savedBase.Faces, faceType => (Base.FaceType)faceType);
        }
        if (savedBase.Cells != null)
        {
            @base.cells = Array.ConvertAll(savedBase.Cells, cellType => (Base.CellType)cellType);
        }
        if (savedBase.Links != null)
        {
            @base.links = savedBase.Links;
        }
        if (savedBase.CellOffset != null)
        {
            @base.cellOffset = new(savedBase.CellOffset.ToUnity());
        }
        if (savedBase.Masks != null)
        {
            @base.masks = savedBase.Masks;
        }
        if (savedBase.IsGlass != null)
        {
            @base.isGlass = Array.ConvertAll(savedBase.IsGlass, num => num == 1);
        }
        if (savedBase.Anchor != null)
        {
            @base.anchor = new(savedBase.Anchor.ToUnity());
        }
    }

    public static string ToString(this SavedBase savedBase)
    {
        StringBuilder builder = new();
        if (savedBase.BaseShape != null)
        {
            builder.AppendLine($"BaseShape: [{string.Join(";", savedBase.BaseShape)}]");
        }
        if (savedBase.Faces != null)
        {
            builder.AppendLine($"Faces: {string.Join(", ", savedBase.Faces)}");
        }
        if (savedBase.Cells != null)
        {
            builder.AppendLine($"Cells: {string.Join(", ", savedBase.Cells)}");
        }
        if (savedBase.Links != null)
        {
            builder.AppendLine($"Links: {string.Join(", ", savedBase.Links)}");
        }
        if (savedBase.CellOffset != null)
        {
            builder.AppendLine($"CellOffset: [{string.Join(";", savedBase.CellOffset)}]");
        }
        if (savedBase.Masks != null)
        {
            builder.AppendLine($"Masks: {string.Join(", ", savedBase.Masks)}");
        }
        if (savedBase.IsGlass != null)
        {
            builder.AppendLine($"IsGlass: {string.Join(", ", savedBase.IsGlass)}");
        }
        if (savedBase.Anchor != null)
        {
            builder.AppendLine($"CellOffset: [{string.Join(";", savedBase.Anchor)}]");
        }
        return builder.ToString();
    }
}

public static class NitroxInteriorPiece
{
    public static InteriorPieceEntity From(IBaseModule module)
    {
        InteriorPieceEntity interiorPiece = InteriorPieceEntity.MakeEmpty();
        GameObject gameObject = (module as Component).gameObject;
        if (gameObject && gameObject.TryGetComponent(out PrefabIdentifier identifier))
        {
            interiorPiece.ClassId = identifier.ClassId;
            // TODO: Fix techtype not being found out
            interiorPiece.TechType = CraftData.entClassTechTable.GetOrDefault(identifier.ClassId, TechType.None).ToDto();
        }
        else
        {
            Log.Warn($"Couldn't find an identifier for the interior piece {module.GetType()}");
        }

        if (NitroxEntity.TryGetEntityFrom(gameObject, out NitroxEntity entity))
        {
            interiorPiece.Id = entity.Id;
        }
        else
        {
            Log.Warn($"Couldn't find a NitroxEntity for the interior piece {module.GetType()}");
        }
        if (gameObject.TryGetComponentInParent(out Base parentBase) &&
            NitroxEntity.TryGetEntityFrom(parentBase.gameObject, out NitroxEntity parentEntity))
        {
            interiorPiece.ParentId = parentEntity.Id;
        }

        // TODO: Verify if this is necessary or not
        // EDIT: This is most likely not, apart from the PlanterEntity which is important
        switch (module)
        {
            case LargeRoomWaterPark largeRoomWaterPark:
                PlanterEntity leftPlanter = new(interiorPiece.Id.Increment(), interiorPiece.Id);
                PlanterEntity rightPlanter = new(leftPlanter.Id.Increment(), interiorPiece.Id);
                interiorPiece.ChildEntities.Add(leftPlanter);
                interiorPiece.ChildEntities.Add(rightPlanter);
                // TODO: Eventually add the children items as entities
                interiorPiece.Constructed = largeRoomWaterPark.constructed;
                break;
            case WaterPark waterPark:
                interiorPiece.Constructed = waterPark.constructed;
                break;
            case FiltrationMachine filtrationMachine:
                interiorPiece.Constructed = filtrationMachine.constructed;
                break;
            case BaseUpgradeConsole baseUpgradeConsole:
                interiorPiece.Constructed = baseUpgradeConsole.constructed;
                break;
            case BaseNuclearReactor baseNuclearReactor:
                interiorPiece.Constructed = baseNuclearReactor.constructed;
                break;
            case BaseBioReactor baseBioReactor:
                interiorPiece.Constructed = baseBioReactor.constructed;
                break;
        }

        interiorPiece.BaseFace = module.moduleFace.ToDto();

        return interiorPiece;
    }

    public static string ToString(this SavedInteriorPiece savedInteriorPiece)
    {
        return $"SavedInteriorPiece [ClassId: {savedInteriorPiece.ClassId}, Face: [{savedInteriorPiece.BaseFace.Cell};{savedInteriorPiece.BaseFace.Direction}], Constructed: {savedInteriorPiece.Constructed}]";
    }
}

public static class NitroxModule
{
    public static void FillObject(this ModuleEntity moduleEntity, Constructable constructable)
    {
        moduleEntity.ClassId = constructable.GetComponent<PrefabIdentifier>().ClassId;

        if (NitroxEntity.TryGetEntityFrom(constructable.gameObject, out NitroxEntity entity))
        {
            moduleEntity.Id = entity.Id;
        }
        if (constructable.TryGetComponentInParent(out Base parentBase) &&
            NitroxEntity.TryGetEntityFrom(parentBase.gameObject, out NitroxEntity parentEntity))
        {
            moduleEntity.ParentId = parentEntity.Id;
        }
        moduleEntity.LocalPosition = constructable.transform.localPosition.ToDto();
        moduleEntity.LocalRotation = constructable.transform.localRotation.ToDto();
        moduleEntity.LocalScale = constructable.transform.localScale.ToDto();
        moduleEntity.TechType = constructable.techType.ToDto();
        moduleEntity.ConstructedAmount = constructable.constructedAmount;
        moduleEntity.IsInside = constructable.isInside;
    }

    public static void MoveTransform(this ModuleEntity moduleEntity, Transform transform)
    {
        transform.localPosition = moduleEntity.LocalPosition.ToUnity();
        transform.localRotation = moduleEntity.LocalRotation.ToUnity();
        transform.localScale = moduleEntity.LocalScale.ToUnity();
    }

    public static ModuleEntity From(Constructable constructable)
    {
        ModuleEntity module = ModuleEntity.MakeEmpty();
        module.FillObject(constructable);
        return module;
    }

    public static string ToString(this SavedModule savedModule)
    {
        return $"SavedModule [ClassId: {savedModule.ClassId}, NitroxId: {savedModule.NitroxId}, Position: {savedModule.Position}, Rotation: {savedModule.Rotation}, LocalScale: {savedModule.LocalScale}, ConstructedAmount: {savedModule.ConstructedAmount}, IsInside: {savedModule.IsInside}]";
    }
}

public static class NitroxGhost
{
    public static GhostEntity From(ConstructableBase constructableBase)
    {
        GhostEntity ghost = GhostEntity.MakeEmpty();

        ghost.FillObject(constructableBase);
        if (constructableBase.moduleFace.HasValue)
        {
            ghost.BaseFace = constructableBase.moduleFace.Value.ToDto();
        }

        ghost.SavedBase = NitroxBase.From(constructableBase.model.GetComponent<Base>());
        if (constructableBase.name.Equals("BaseDeconstructable(Clone)"))
        {
            ghost.TechType = constructableBase.techType.ToDto();
        }

        if (constructableBase.TryGetComponentInChildren(out BaseGhost baseGhost, true))
        {
            ghost.Metadata = NitroxGhostMetadata.GetMetadataForGhost(baseGhost);
        }

        return ghost;
    }

    public static string ToString(this GhostEntity ghostEntity)
    {
        return $"SavedGhost [ClassId: {ghostEntity.ClassId}, NitroxId: {ghostEntity.Id}, ParentId: {ghostEntity.ParentId}, LocalPosition: {ghostEntity.LocalPosition}, LocalRotation: {ghostEntity.LocalRotation}, LocalScale: {ghostEntity.LocalScale}, ConstructedAmount: {ghostEntity.ConstructedAmount}, IsInside: {ghostEntity.IsInside}, ModuleFace: [{ghostEntity.BaseFace}]]";
    }
}
