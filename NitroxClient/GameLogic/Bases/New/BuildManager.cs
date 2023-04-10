using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
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

    public static bool TryTransferIdFromGhostToModule(BaseGhost baseGhost, NitroxId id, ConstructableBase constructableBase)
    {
        Log.Debug($"TryTransferIdFromGhostToModule({baseGhost},{id})");
        Base.Face? face = null;
        bool isWaterPark = baseGhost is BaseAddWaterPark;
        bool isMapRoomGhost = baseGhost is BaseAddMapRoomGhost;
        // Only three types of ghost which spawn a module
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
                    // Moonpools are a very specific case, we tweak them to work as modules (while they're not)
                    baseGhost.targetBase.gameObject.EnsureComponent<MoonpoolManager>().RegisterMoonpool(constructableBase.transform, id);
                    return true;

                case TechType.BaseMapRoom:
                    // In the case the ghost is under a BaseDeconstructable, this is a good way to identify a MapRoom
                    face = new(GetMapRoomFunctionalityCell(baseGhost), 0);
                    isMapRoomGhost = true;
                    break;

                default:
                    return false;
            }
        }

        if (face.HasValue)
        {
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
                    return true;
                }
                Log.Error($"Couldn't find MapRoomFunctionality of built MapRoom (cell: {face.Value.cell})");
                return false;
            }

            IBaseModule module = baseGhost.targetBase.GetModule(face.Value);
            if (module != null)
            {
                // If the WaterPark is higher than one, it means that the newly built WaterPark will be merged with one that already has a NitroxEntity
                if (module is WaterPark waterPark && waterPark.height > 1)
                {
                    Log.Debug($"Found WaterPark higher than 1 {waterPark.height}, not transferring NitroxEntity to it");
                    return true;
                }

                Log.Debug($"Successfully transferred NitroxEntity to {module} [{id}]");
                NitroxEntity.SetNewId((module as Component).gameObject, id);
                return true;
            }
            else
            {
                // When a WaterPark is merged with another one, we won't find its module but we don't care about that
                if (isWaterPark)
                {
                    return false;
                }
                Log.Error("Couldn't find the module's GameObject of built interior piece when transfering its NitroxEntity to the module.");
            }
        }
        else
        {
            Log.Error($"No face could be found for ghost {baseGhost}");
        }

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


// TODO: Add further metadata in modules and interior pieces

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

        buildEntity.Position = targetBase.transform.position.ToDto();
        buildEntity.Rotation = targetBase.transform.rotation.ToDto();
        buildEntity.LocalScale = targetBase.transform.localScale.ToDto();

        buildEntity.SavedBase = NitroxBase.From(targetBase);
        buildEntity.ChildEntities.AddRange(GetChildEntities(targetBase, entity.Id));
        
        return buildEntity;
    }

    public static List<Entity> GetChildEntities(Base targetBase, NitroxId baseId)
    {
        List<Entity> childEntities = new();
        foreach (InteriorPieceEntity interiorPieceEntity in SaveInteriorPieces(targetBase))
        {
            childEntities.Add(interiorPieceEntity);
        }
        foreach (ModuleEntity moduleEntity in SaveModules(targetBase.transform))
        {
            childEntities.Add(moduleEntity);
        }
        foreach (GhostEntity ghostEntity in SaveGhosts(targetBase.transform))
        {
            childEntities.Add(ghostEntity);
        }
        if (targetBase.TryGetComponent(out MoonpoolManager nitroxMoonpool))
        {
            foreach (MoonpoolEntity moonpoolEntity in nitroxMoonpool.GetSavedMoonpools())
            {
                childEntities.Add(moonpoolEntity);
            }
        }
        // TODO: Add MapRoomFunctionality as children
        foreach (Entity childEntity in childEntities)
        {
            childEntity.ParentId = baseId;
        }
        return childEntities;
    }
    
    public static IEnumerator CreateBuild(BuildEntity buildEntity, TaskResult<Optional<GameObject>> result = null)
    {
        GameObject newBase = UnityEngine.Object.Instantiate(BaseGhost._basePrefab, LargeWorldStreamer.main.globalRoot.transform, buildEntity.Position.ToUnity(), buildEntity.Rotation.ToUnity(), buildEntity.LocalScale.ToUnity(), false);
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

    public static void ApplyTo(this BuildEntity buildEntity, Base @base)
    {
        NitroxEntity.SetNewId(@base.gameObject, buildEntity.Id);
        buildEntity.SavedBase.ApplyTo(@base);
    }

    public static IEnumerator ApplyToAsync(this BuildEntity buildEntity, Base @base)
    {
        buildEntity.ApplyTo(@base);
        yield return buildEntity.RestoreInteriorPieces(@base);
        yield return buildEntity.RestoreModules(@base);
    }

    public static IEnumerator RestoreInteriorPieces(this BuildEntity buildEntity, Base @base)
    {
        foreach (InteriorPieceEntity interiorPiece in buildEntity.ChildEntities.OfType<InteriorPieceEntity>())
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(interiorPiece.ClassId);
            yield return request;
            if (!request.TryGetPrefab(out GameObject prefab))
            {
                Log.Debug($"Couldn't find a prefab for interior piece of ClassId {interiorPiece.ClassId}");
                continue;
            }
            Base.Face face = interiorPiece.BaseFace.ToUnity();
            face.cell += @base.GetAnchor();
            GameObject moduleObject = @base.SpawnModule(prefab, face);
            if (moduleObject)
            {
                NitroxEntity.SetNewId(moduleObject, interiorPiece.Id);
            }
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
        moduleTransform.localPosition = moduleEntity.Position.ToUnity();
        moduleTransform.localRotation = moduleEntity.Rotation.ToUnity();
        moduleTransform.localScale = moduleEntity.LocalScale.ToUnity();

        Constructable constructable = moduleObject.GetComponent<Constructable>();
        constructable.SetIsInside(moduleEntity.IsInside);
        // TODO: If IsInside is false, maybe set a null environment
        SkyEnvironmentChanged.Send(moduleObject, moduleObject.GetComponentInParent<SubRoot>(true));
        constructable.constructedAmount = moduleEntity.ConstructedAmount;
        constructable.SetState(moduleEntity.ConstructedAmount >= 1f, false);
        constructable.UpdateMaterial();
        NitroxEntity.SetNewId(moduleObject, moduleEntity.Id);
        result?.Set(moduleObject);
    }

    public static IEnumerator RestoreModules(Transform parent, IEnumerable<ModuleEntity> modules)
    {
        foreach (ModuleEntity moduleEntity in modules)
        {
            yield return RestoreModule(parent, moduleEntity);
        }
    }

    public static IEnumerator RestoreModules(this BuildEntity buildEntity, Base @base)
    {
        yield return RestoreModules(@base.transform, buildEntity.ChildEntities.OfType<ModuleEntity>());
    }

    public static IEnumerator RestoreGhost(Transform parent, GhostEntity ghostEntity, TaskResult<Optional<GameObject>> result = null)
    {
        Log.Debug($"Restoring ghost {NitroxGhost.ToString(ghostEntity)}");
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

        // TODO: Fix hatch ghosts showing the wrong model
        // TODO: Fix ghost visual glitch (probably a duplicate model) (black ghost)
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
        yield return moonpoolManager.SpawnVehicles();
        moonpoolManager.OnPostRebuildGeometry(@base);
        Log.Debug($"Restored moonpools: {moonpoolManager.GetSavedMoonpools().Count}");
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
        // EDIT: This is most likely not
        switch (module)
        {
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
        moduleEntity.Position = constructable.transform.localPosition.ToDto();
        moduleEntity.Rotation = constructable.transform.localRotation.ToDto();
        moduleEntity.LocalScale = constructable.transform.localScale.ToDto();
        moduleEntity.ConstructedAmount = constructable.constructedAmount;
        moduleEntity.IsInside = constructable.isInside;
    }

    public static void MoveTransform(this ModuleEntity moduleEntity, Transform transform)
    {
        transform.localPosition = moduleEntity.Position.ToUnity();
        transform.localRotation = moduleEntity.Rotation.ToUnity();
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

        if (constructableBase.TryGetComponentInChildren(out BaseGhost baseGhost))
        {
            ghost.Metadata = NitroxGhostMetadata.GetMetadataForGhost(baseGhost);
        }

        return ghost;
    }

    public static string ToString(this GhostEntity ghostEntity)
    {
        return $"SavedGhost [ClassId: {ghostEntity.ClassId}, NitroxId: {ghostEntity.Id}, ParentId: {ghostEntity.ParentId}, Position: {ghostEntity.Position}, Rotation: {ghostEntity.Rotation}, LocalScale: {ghostEntity.LocalScale}, ConstructedAmount: {ghostEntity.ConstructedAmount}, IsInside: {ghostEntity.IsInside}, ModuleFace: [{ghostEntity.BaseFace}]]";
    }
}
