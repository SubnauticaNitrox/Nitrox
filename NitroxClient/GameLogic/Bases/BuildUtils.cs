using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases;

public static class BuildUtils
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
        if (baseGhost is BaseAddFaceGhost faceGhost && faceGhost.modulePrefab ||
            baseGhost is BaseAddModuleGhost moduleGhost && moduleGhost.modulePrefab ||
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

    public static MapRoomEntity GetMapRoomEntityFrom(MapRoomFunctionality mapRoomFunctionality, Base @base, NitroxId id, NitroxId parentId)
    {
        Int3 mapRoomCell = @base.NormalizeCell(@base.WorldToGrid(mapRoomFunctionality.transform.position));
        return new(id, parentId, mapRoomCell.ToDto());
    }

    public static List<GlobalRootEntity> GetGlobalRootChildren(Transform globalRoot)
    {
        List<GlobalRootEntity> entities = new();
        foreach (Transform child in globalRoot)
        {
            if (child.TryGetComponent(out Base @base))
            {
                entities.Add(BuildEntitySpawner.From(@base));
            }
            else if (child.TryGetComponent(out Constructable constructable))
            {
                if (constructable is ConstructableBase constructableBase)
                {
                    entities.Add(GhostEntitySpawner.From(constructableBase));
                    continue;
                }
                entities.Add(ModuleEntitySpawner.From(constructable));
            }
        }
        return entities;
    }

    public static List<Entity> GetChildEntities(Base targetBase, NitroxId baseId)
    {
        List<Entity> childEntities = new();

        foreach (Transform transform in targetBase.transform)
        {
            if (transform.TryGetComponent(out MapRoomFunctionality mapRoomFunctionality))
            {
                if (!mapRoomFunctionality.TryGetNitroxId(out NitroxId mapRoomId))
                {
                    continue;
                }
                Log.Debug($"MapRoom found {mapRoomId} in {mapRoomFunctionality.gameObject} under {mapRoomFunctionality.transform.parent}");
                childEntities.Add(GetMapRoomEntityFrom(mapRoomFunctionality, targetBase, mapRoomId, baseId));
            }
            else if (transform.TryGetComponent(out IBaseModule baseModule))
            {
                // IBaseModules without a NitroxEntity are related to BaseDeconstructable and are saved with their ghost
                if (!(baseModule as MonoBehaviour).GetComponent<NitroxEntity>())
                {
                    continue;
                }
                MonoBehaviour moduleMB = baseModule as MonoBehaviour;
                Log.Debug($"Base module found: {baseModule.GetType().FullName}  in {moduleMB.gameObject} under {moduleMB.transform.parent}");
                childEntities.Add(InteriorPieceEntitySpawner.From(baseModule));
            }
            else if (transform.TryGetComponent(out Constructable constructable))
            {
                if (constructable is ConstructableBase constructableBase)
                {
                    Log.Debug($"BaseConstructable found: {constructableBase.name}");
                    childEntities.Add(GhostEntitySpawner.From(constructableBase));
                    continue;
                }
                Log.Debug($"Constructable found: {constructable.name}");
                childEntities.Add(ModuleEntitySpawner.From(constructable));
            }
        }

        if (targetBase.TryGetComponent(out MoonpoolManager nitroxMoonpool))
        {
            childEntities.AddRange(nitroxMoonpool.GetSavedMoonpools());
        }

        // Making sure that childEntities are correctly parented
        foreach (Entity childEntity in childEntities)
        {
            childEntity.ParentId = baseId;
        }
        return childEntities;
    }
}
