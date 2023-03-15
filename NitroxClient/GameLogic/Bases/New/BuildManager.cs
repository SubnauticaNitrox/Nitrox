using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
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
        }

        face = default;
        return false;
    }

    public static bool TryTransferIdFromGhostToModule(BaseGhost baseGhost, NitroxId id, ConstructableBase constructableBase)
    {
        Log.Debug($"TryTransferIdFromGhostToModule({baseGhost},{id})");
        Base.Face? face = null;
        bool isWaterPark = baseGhost is BaseAddWaterPark;
        // Only three types of ghost which spawn a module
        if ((baseGhost is BaseAddFaceGhost faceGhost && faceGhost.modulePrefab) ||
            (baseGhost is BaseAddModuleGhost moduleGhost && moduleGhost.modulePrefab) ||
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
        // Edge case that happens when a Deconstructed WaterPark is built onto another deconstructed WaterPark that has its module
        // A new module will be created by the current Deconstructed WaterPark which is the one we'll be aiming at
        else if (constructableBase.techType.Equals(TechType.BaseWaterPark) && !isWaterPark)
        {
            IBaseModuleGeometry baseModuleGeometry = constructableBase.GetComponentInChildren<IBaseModuleGeometry>(true);
            if (baseModuleGeometry != null)
            {
                face = baseModuleGeometry.geometryFace;
            }
        }
        // Moonpools are a very specific case, we tweak them to work as modules (while they're not)
        else if (constructableBase.techType.Equals(TechType.BaseMoonpool))
        {
            baseGhost.targetBase.gameObject.EnsureComponent<MoonpoolManager>().RegisterMoonpool(constructableBase.transform, id);
            return true;
        }
        else
        {
            return false;
        }

        if (face.HasValue)
        {
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
    public static SavedBuild From(Base targetBase)
    {
        SavedBuild savedBuild = new();
        if (NitroxEntity.TryGetEntityFrom(targetBase.gameObject, out NitroxEntity entity))
        {
            savedBuild.NitroxId = entity.Id;
        }

        savedBuild.Position = targetBase.transform.position.ToDto();
        savedBuild.Rotation = targetBase.transform.rotation.ToDto();
        savedBuild.LocalScale = targetBase.transform.localScale.ToDto();

        savedBuild.Base = NitroxBase.From(targetBase);

        savedBuild.InteriorPieces = SaveInteriorPieces(targetBase);
        savedBuild.Modules = SaveModules(targetBase.transform);
        savedBuild.Ghosts = SaveGhosts(targetBase.transform);

        if (targetBase.TryGetComponent(out MoonpoolManager nitroxMoonpool))
        {
            savedBuild.Moonpools = nitroxMoonpool.GetSavedMoonpools();
        }
        return savedBuild;
    }
    public static IEnumerator CreateBuild(SavedBuild savedBuild)
    {
        GameObject newBase = UnityEngine.Object.Instantiate(BaseGhost._basePrefab, LargeWorldStreamer.main.globalRoot.transform, savedBuild.Position.ToUnity(), savedBuild.Rotation.ToUnity(), savedBuild.LocalScale.ToUnity(), false);
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
        yield return savedBuild.ApplyToAsync(@base);
        newBase.SetActive(true);
        yield return savedBuild.RestoreGhosts(@base);
        @base.OnProtoDeserialize(null);
        @base.FinishDeserialization();
        yield return savedBuild.RestoreMoonpools(@base);
    }

    private static List<SavedInteriorPiece> SaveInteriorPieces(Base targetBase)
    {
        List<SavedInteriorPiece> interiorPieces = new();
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

    public static List<SavedModule> SaveModules(Transform parent)
    {
        List<SavedModule> savedModules = new();
        foreach (Transform transform in parent)
        {
            if (transform.TryGetComponent(out Constructable constructable) && constructable is not ConstructableBase)
            {
                Log.Debug($"Constructable found: {constructable.name}");
                savedModules.Add(NitroxModule.From(constructable));
            }
        }
        return savedModules;
    }

    public static List<SavedGhost> SaveGhosts(Transform parent)
    {
        List<SavedGhost> savedGhosts = new();
        foreach (Transform transform in parent)
        {
            if (transform.TryGetComponent(out Constructable constructable) && constructable is ConstructableBase constructableBase)
            {
                Log.Debug($"BaseConstructable found: {constructableBase.name}");
                savedGhosts.Add(NitroxGhost.From(constructableBase));
            }
        }
        return savedGhosts;
    }

    public static void ApplyTo(this SavedBuild savedBuild, Base @base)
    {
        NitroxEntity.SetNewId(@base.gameObject, savedBuild.NitroxId);
        savedBuild.Base.ApplyTo(@base);
    }

    public static IEnumerator ApplyToAsync(this SavedBuild savedBuild, Base @base)
    {
        savedBuild.ApplyTo(@base);
        yield return savedBuild.RestoreInteriorPieces(@base);
        yield return savedBuild.RestoreModules(@base);
    }

    public static IEnumerator RestoreInteriorPieces(this SavedBuild savedBuild, Base @base)
    {
        foreach (SavedInteriorPiece interiorPiece in savedBuild.InteriorPieces)
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
                NitroxEntity.SetNewId(moduleObject, interiorPiece.NitroxId);
            }
        }
    }

    public static IEnumerator RestoreModule(Transform parent, SavedModule savedModule)
    {
        Log.Debug($"Restoring module {savedModule.ClassId}");
        IPrefabRequest request = PrefabDatabase.GetPrefabAsync(savedModule.ClassId);
        yield return request;
        if (!request.TryGetPrefab(out GameObject prefab))
        {
            Log.Debug($"Couldn't find a prefab for module of ClassId {savedModule.ClassId}");
            yield break;
        }
        GameObject moduleObject = GameObject.Instantiate(prefab);
        Transform moduleTransform = moduleObject.transform;
        moduleTransform.parent = parent;
        moduleTransform.localPosition = savedModule.Position.ToUnity();
        moduleTransform.localRotation = savedModule.Rotation.ToUnity();
        moduleTransform.localScale = savedModule.LocalScale.ToUnity();

        Constructable constructable = moduleObject.GetComponent<Constructable>();
        constructable.SetIsInside(savedModule.IsInside);
        // TODO: If IsInside is false, maybe set a null environment
        SkyEnvironmentChanged.Send(moduleObject, moduleObject.GetComponentInParent<SubRoot>(true));
        constructable.constructedAmount = savedModule.ConstructedAmount;
        constructable.SetState(savedModule.ConstructedAmount >= 1f, false);
        constructable.UpdateMaterial();
        NitroxEntity.SetNewId(moduleObject, savedModule.NitroxId);
    }

    public static IEnumerator RestoreModules(Transform parent, IList<SavedModule> modules)
    {
        foreach (SavedModule savedModule in modules)
        {
            yield return RestoreModule(parent, savedModule);
        }
    }

    public static IEnumerator RestoreModules(this SavedBuild savedBuild, Base @base)
    {
        yield return RestoreModules(@base.transform, savedBuild.Modules);
    }

    public static IEnumerator RestoreGhost(Transform parent, SavedGhost savedGhost)
    {
        Log.Debug($"Restoring ghost {NitroxGhost.ToString(savedGhost)}");
        IPrefabRequest request = PrefabDatabase.GetPrefabAsync(savedGhost.ClassId);
        yield return request;
        if (!request.TryGetPrefab(out GameObject prefab))
        {
            Log.Debug($"Couldn't find a prefab for module of ClassId {savedGhost.ClassId}");
            yield break;
        }
        bool isInBase = parent.TryGetComponent(out Base @base);

        GameObject ghostObject = GameObject.Instantiate(prefab);
        Transform ghostTransform = ghostObject.transform;
        savedGhost.MoveTransform(ghostTransform);

        ConstructableBase constructableBase = ghostObject.GetComponent<ConstructableBase>();
        GameObject ghostModel = constructableBase.model;
        BaseGhost baseGhost = ghostModel.GetComponent<BaseGhost>();
        Base ghostBase = ghostModel.GetComponent<Base>();
        bool isBaseDeconstructable = ghostObject.name.Equals("BaseDeconstructable(Clone)");
        if (isBaseDeconstructable && savedGhost.TechType != null)
        {
            constructableBase.techType = savedGhost.TechType.ToUnity();
        }
        constructableBase.constructedAmount = savedGhost.ConstructedAmount;

        baseGhost.SetupGhost();

        yield return NitroxGhostMetadata.ApplyMetadataToGhost(baseGhost, savedGhost.Metadata, @base);

        // TODO: Fix black ghost
        // Necessary to wait for BaseGhost.Start()
        yield return null;
        // Verify that the metadata didn't destroy the GameObject
        if (!ghostObject)
        {
            yield break;
        }

        savedGhost.Base.ApplyTo(ghostBase);
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
            savedGhost.MoveTransform(ghostTransform);
        }

        constructableBase.SetState(false, false);
        constructableBase.UpdateMaterial();

        NitroxGhostMetadata.LateApplyMetadataToGhost(baseGhost, savedGhost.Metadata);

        NitroxEntity.SetNewId(ghostObject, savedGhost.NitroxId);
    }

    public static IEnumerator RestoreGhosts(Transform parent, IList<SavedGhost> ghosts)
    {
        // TODO: Fix ghost visual glitch (probably a duplicate model)
        foreach (SavedGhost savedGhost in ghosts)
        {
            yield return RestoreGhost(parent, savedGhost);
        }
    }

    public static IEnumerator RestoreGhosts(this SavedBuild savedBuild, Base @base)
    {
        yield return RestoreGhosts(@base.transform, savedBuild.Ghosts);
    }

    public static IEnumerator RestoreMoonpools(this SavedBuild savedBuild, Base @base)
    {
        MoonpoolManager moonpoolManager = @base.gameObject.EnsureComponent<MoonpoolManager>();
        moonpoolManager.LoadSavedMoonpools(savedBuild.Moonpools);
        moonpoolManager.OnPostRebuildGeometry(@base);
        Log.Debug($"Restored moonpools: {moonpoolManager.GetSavedMoonpools().Count}");
        yield break;
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
    public static SavedInteriorPiece From(IBaseModule module)
    {
        SavedInteriorPiece interiorPiece = new();
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
            interiorPiece.NitroxId = entity.Id;
        }
        else
        {
            Log.Warn($"Couldn't find a NitroxEntity for the interior piece {module.GetType()}");
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
    public static void FillObject(this SavedModule savedModule, Constructable constructable)
    {
        savedModule.ClassId = constructable.GetComponent<PrefabIdentifier>().ClassId;

        if (NitroxEntity.TryGetEntityFrom(constructable.gameObject, out NitroxEntity entity))
        {
            savedModule.NitroxId = entity.Id;
        }
        savedModule.Position = constructable.transform.localPosition.ToDto();
        savedModule.Rotation = constructable.transform.localRotation.ToDto();
        savedModule.LocalScale = constructable.transform.localScale.ToDto();
        savedModule.ConstructedAmount = constructable.constructedAmount;
        savedModule.IsInside = constructable.isInside;
    }

    public static void MoveTransform(this SavedModule savedModule, Transform transform)
    {
        transform.localPosition = savedModule.Position.ToUnity();
        transform.localRotation = savedModule.Rotation.ToUnity();
        transform.localScale = savedModule.LocalScale.ToUnity();
    }

    public static SavedModule From(Constructable constructable)
    {
        SavedModule module = new();
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
    public static SavedGhost From(ConstructableBase constructableBase)
    {
        SavedGhost ghost = new();

        ghost.FillObject(constructableBase);
        if (constructableBase.moduleFace.HasValue)
        {
            ghost.BaseFace = constructableBase.moduleFace.Value.ToDto();
        }
        
        ghost.Base = NitroxBase.From(constructableBase.model.GetComponent<Base>());
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

    public static string ToString(this SavedGhost savedGhost)
    {
        return $"SavedGhost [ClassId: {savedGhost.ClassId}, NitroxId: {savedGhost.NitroxId}, Position: {savedGhost.Position}, Rotation: {savedGhost.Rotation}, LocalScale: {savedGhost.LocalScale}, ConstructedAmount: {savedGhost.ConstructedAmount}, IsInside: {savedGhost.IsInside}, ModuleFace: [{savedGhost.BaseFace.Cell};{savedGhost.BaseFace.Direction}]]";
    }
}
