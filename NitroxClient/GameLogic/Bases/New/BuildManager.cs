using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Bases.New;

public class BuildManager
{
    
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
            savedBuild.NitroxId = $"{entity.Id}";
        }

        savedBuild.Position = targetBase.transform.position.ToDto();
        savedBuild.Rotation = targetBase.transform.rotation.ToDto();
        savedBuild.LocalScale = targetBase.transform.localScale.ToDto();

        savedBuild.Base = NitroxBase.From(targetBase);

        savedBuild.InteriorPieces = SaveInteriorPieces(targetBase).ToArray();
        savedBuild.Modules = SaveModules(targetBase.transform).ToArray();
        savedBuild.Ghosts = SaveGhosts(targetBase.transform).ToArray();

        return savedBuild;
    }

    private static List<SavedInteriorPiece> SaveInteriorPieces(Base targetBase)
    {
        List<SavedInteriorPiece> interiorPieces = new();
        foreach (IBaseModule module in targetBase.GetComponentsInChildren<IBaseModule>(true))
        {
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
        if (Guid.TryParse(savedBuild.NitroxId, out Guid id))
        {
            NitroxEntity.SetNewId(@base.gameObject, new(id));
        }

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
            @base.SpawnModule(prefab, new(interiorPiece.BaseFace.Cell.ToUnity() + @base.anchor, (Base.Direction)interiorPiece.BaseFace.Direction));
        }
    }

    public static IEnumerator RestoreModules(Transform parent, IList<SavedModule> modules)
    {
        foreach (SavedModule savedModule in modules)
        {
            Log.Debug($"Restoring module {savedModule.ClassId}");
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(savedModule.ClassId);
            yield return request;
            if (!request.TryGetPrefab(out GameObject prefab))
            {
                Log.Debug($"Couldn't find a prefab for module of ClassId {savedModule.ClassId}");
                continue;
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
        }
    }

    public static IEnumerator RestoreModules(this SavedBuild savedBuild, Base @base)
    {
        yield return RestoreModules(@base.transform, savedBuild.Modules);
    }

    public static IEnumerator RestoreGhosts(Transform parent, IList<SavedGhost> ghosts)
    {
        // TODO: Fix ghost visual glitch (probably a duplicate model)
        foreach (SavedGhost savedGhost in ghosts)
        {
            Log.Debug($"Restoring ghost {savedGhost.ClassId}");
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(savedGhost.ClassId);
            yield return request;
            if (!request.TryGetPrefab(out GameObject prefab))
            {
                Log.Debug($"Couldn't find a prefab for module of ClassId {savedGhost.ClassId}");
                continue;
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

            baseGhost.SetupGhost();
            NitroxGhostMetadata.ApplyMetadataToGhost(baseGhost, savedGhost.Metadata);

            // Necessary to wait for BaseGhost.Start()
            yield return null;

            foreach (Renderer renderer in constructableBase.ghostRenderers)
            {
                Log.Debug($"{renderer.name} (under {renderer.transform.parent.name}) bounds: {renderer.bounds}");
            }

            if (isInBase)
            {
                @base.SetPlacementGhost(baseGhost);
                baseGhost.targetBase = @base;
            }
            else
            {
                ghostTransform.parent = parent;
            }

            savedGhost.Base.ApplyTo(ghostBase);
            ghostBase.OnProtoDeserialize(null);
            ghostBase.FinishDeserialization();

            Log.Debug($"{NitroxBase.From(ghostBase)}");

            if (!isBaseDeconstructable)
            {
                baseGhost.Place();
            }

            if (isInBase)
            {
                ghostTransform.parent = parent;
                savedGhost.MoveTransform(ghostTransform);
            }

            constructableBase.constructedAmount = savedGhost.ConstructedAmount;
            constructableBase.SetState(false, false);
            constructableBase.UpdateMaterial();
        }
    }

    public static IEnumerator RestoreGhosts(this SavedBuild savedBuild, Base @base)
    {
        yield return RestoreGhosts(@base.transform, savedBuild.Ghosts);
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

        // TODO: Verify if this is necessary or not
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
            savedModule.NitroxId = $"{entity.Id}";
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
