using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata.Bases;
using NitroxModel_Subnautica.DataStructures;
using System.Collections;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Bases;

public static class GhostMetadataApplier
{
    public static T From<T>(BaseGhost baseGhost) where T : GhostMetadata, new()
    {
        T metadata = new()
        {
            TargetOffset = baseGhost.targetOffset.ToDto()
        };
        return metadata;
    }

    public static void ApplyBasicMetadataTo(this GhostMetadata ghostMetadata, BaseGhost baseGhost)
    {
        baseGhost.targetOffset = ghostMetadata.TargetOffset.ToUnity();
    }

    public static GhostMetadata GetMetadataForGhost(BaseGhost baseGhost)
    {
        // Specific case in which a piece was deconstructed and resulted in a BaseDeconstructable with a normal BaseGhost
        if (BuildUtils.IsUnderBaseDeconstructable(baseGhost))
        {
            return GetBaseDeconstructableMetadata(baseGhost);
        }

        GhostMetadata metadata = baseGhost switch
        {
            BaseAddWaterPark or BaseAddPartitionDoorGhost or BaseAddModuleGhost or BaseAddFaceGhost => GetBaseAnchoredFaceMetadata(baseGhost),
            BaseAddPartitionGhost => GetBaseAnchoredCellMetadata(baseGhost),
            _ => From<GhostMetadata>(baseGhost),
        };
        return metadata;
    }

    public static IEnumerator ApplyMetadataToGhost(BaseGhost baseGhost, EntityMetadata entityMetadata, Base @base)
    {
        if (entityMetadata is not GhostMetadata ghostMetadata)
        {
            Log.Error($"Trying to apply metadata to a ghost that is not of type {nameof(GhostMetadata)} : [{entityMetadata.GetType()}]");
            yield break;
        }

        if (BuildUtils.IsUnderBaseDeconstructable(baseGhost, true) && entityMetadata is BaseDeconstructableGhostMetadata deconstructableMetadata)
        {
            yield return deconstructableMetadata.ApplyBaseDeconstructableMetadataTo(baseGhost, @base);
            yield break;
        }

        switch (baseGhost)
        {
            case BaseAddWaterPark:
            case BaseAddPartitionDoorGhost:
            case BaseAddModuleGhost:
            case BaseAddFaceGhost:
                if (ghostMetadata is BaseAnchoredFaceGhostMetadata faceMetadata)
                {
                    faceMetadata.ApplyBaseAnchoredFaceMetadataTo(baseGhost);
                }

                break;
            case BaseAddPartitionGhost:
                if (ghostMetadata is BaseAnchoredCellGhostMetadata cellMetadata)
                {
                    cellMetadata.ApplyBaseAnchoredCellMetadataTo(baseGhost);
                }

                break;
            default:
                ghostMetadata.ApplyBasicMetadataTo(baseGhost);
                break;
        }
    }

    // Specific metadata getters and appliers

    public static BaseAnchoredCellGhostMetadata GetBaseAnchoredCellMetadata(BaseGhost baseGhost)
    {
        BaseAnchoredCellGhostMetadata metadata = From<BaseAnchoredCellGhostMetadata>(baseGhost);
        if (baseGhost is BaseAddPartitionGhost ghost && ghost.anchoredCell.HasValue)
        {
            metadata.AnchoredCell = ghost.anchoredCell.Value.ToDto();
        }

        return metadata;
    }

    public static void ApplyBaseAnchoredCellMetadataTo(this BaseAnchoredCellGhostMetadata ghostMetadata, BaseGhost baseGhost)
    {
        ApplyBasicMetadataTo(ghostMetadata, baseGhost);
        if (ghostMetadata.AnchoredCell.HasValue)
        {
            if (baseGhost is BaseAddPartitionGhost ghost)
            {
                ghost.anchoredCell = ghostMetadata.AnchoredCell.Value.ToUnity();
            }
        }
    }

    public static BaseAnchoredFaceGhostMetadata GetBaseAnchoredFaceMetadata(BaseGhost baseGhost)
    {
        BaseAnchoredFaceGhostMetadata metadata = From<BaseAnchoredFaceGhostMetadata>(baseGhost);
        metadata.AnchoredFace = baseGhost switch
        {
            BaseAddWaterPark ghost => ghost.anchoredFace?.ToDto(),
            BaseAddPartitionDoorGhost ghost => ghost.anchoredFace?.ToDto(),
            BaseAddModuleGhost ghost => ghost.anchoredFace?.ToDto(),
            BaseAddFaceGhost ghost => ghost.anchoredFace?.ToDto(),
            _ => null
        };
        return metadata;
    }

    public static void ApplyBaseAnchoredFaceMetadataTo(this BaseAnchoredFaceGhostMetadata ghostMetadata, BaseGhost baseGhost)
    {
        ApplyBasicMetadataTo(ghostMetadata, baseGhost);
        if (ghostMetadata.AnchoredFace.HasValue)
        {
            switch (baseGhost)
            {
                case BaseAddWaterPark ghost:
                    ghost.anchoredFace = ghostMetadata.AnchoredFace.Value.ToUnity();
                    break;
                case BaseAddPartitionDoorGhost ghost:
                    ghost.anchoredFace = ghostMetadata.AnchoredFace.Value.ToUnity();
                    break;
                case BaseAddModuleGhost ghost:
                    ghost.anchoredFace = ghostMetadata.AnchoredFace.Value.ToUnity();
                    break;
                case BaseAddFaceGhost ghost:
                    ghost.anchoredFace = ghostMetadata.AnchoredFace.Value.ToUnity();
                    break;
            }
        }
    }

    public static BaseDeconstructableGhostMetadata GetBaseDeconstructableMetadata(BaseGhost baseGhost)
    {
        BaseDeconstructableGhostMetadata metadata = From<BaseDeconstructableGhostMetadata>(baseGhost);
        if (baseGhost.TryGetComponentInParent(out ConstructableBase constructableBase) && constructableBase.moduleFace.HasValue)
        {
            Base.Face moduleFace = constructableBase.moduleFace.Value;
            metadata.ModuleFace = moduleFace.ToDto();
            moduleFace.cell += baseGhost.targetBase.GetAnchor();
            IBaseModule baseModule = baseGhost.targetBase.GetModule(moduleFace);
            if (baseModule != null && (baseModule as MonoBehaviour).TryGetComponent(out PrefabIdentifier identifier))
            {
                metadata.ClassId = identifier.ClassId;
            }
        }

        return metadata;
    }

    public static IEnumerator ApplyBaseDeconstructableMetadataTo(this BaseDeconstructableGhostMetadata ghostMetadata, BaseGhost baseGhost, Base @base)
    {
        ghostMetadata.ApplyBasicMetadataTo(baseGhost);
        baseGhost.DisableGhostModelScripts();
        if (ghostMetadata.ModuleFace.HasValue)
        {
            if (!baseGhost.TryGetComponentInParent(out ConstructableBase constructableBase))
            {
                Log.Error($"Couldn't find an interior piece's parent ConstructableBase to apply a {nameof(BaseDeconstructableGhostMetadata)} to.");
                yield break;
            }

            constructableBase.moduleFace = ghostMetadata.ModuleFace.Value.ToUnity();

            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(ghostMetadata.ClassId);
            yield return request;
            if (!request.TryGetPrefab(out GameObject prefab))
            {
                // Without its module, the ghost will be useless, so we delete it (like in base game)
                Object.Destroy(constructableBase.gameObject);
                Log.Error($"Couldn't find a prefab for module of interior piece of ClassId: {ghostMetadata.ClassId}");
                yield break;
            }

            if (!baseGhost.targetBase)
            {
                baseGhost.targetBase = @base;
            }

            Base.Face face = ghostMetadata.ModuleFace.Value.ToUnity();
            face.cell += baseGhost.targetBase.GetAnchor();
            GameObject moduleObject = baseGhost.targetBase.SpawnModule(prefab, face);
            if (!moduleObject)
            {
                Object.Destroy(constructableBase.gameObject);
                Log.Error("Module couldn't be spawned for interior piece");
                yield break;
            }

            if (moduleObject.TryGetComponent(out IBaseModule baseModule))
            {
                baseModule.constructed = constructableBase.constructedAmount;
            }
        }
    }
}
