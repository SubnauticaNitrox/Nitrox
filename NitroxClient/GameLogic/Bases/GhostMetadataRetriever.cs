using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata.Bases;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases;

/// <summary>
/// Because of the multiple possible types for Ghost components, the retrieving of their metadata is inappropriate for the MetadataExtractor system
/// </summary>
public static class GhostMetadataRetriever
{
    public static GhostMetadata GetMetadataForGhost(BaseGhost baseGhost)
    {
        // Specific case in which a piece was deconstructed and resulted in a BaseDeconstructable with a normal BaseGhost
        if (BuildUtils.IsUnderBaseDeconstructable(baseGhost, true))
        {
            return GetBaseDeconstructableMetadata(baseGhost);
        }

        GhostMetadata metadata = baseGhost switch
        {
            BaseAddWaterPark or BaseAddPartitionDoorGhost or BaseAddModuleGhost or BaseAddFaceGhost => baseGhost.GetBaseAnchoredFaceMetadata(),
            BaseAddPartitionGhost => baseGhost.GetBaseAnchoredCellMetadata(),
            _ => baseGhost.GetMetadata<GhostMetadata>(),
        };
        return metadata;
    }

    public static T GetMetadata<T>(this BaseGhost baseGhost) where T : GhostMetadata, new()
    {
        T metadata = new()
        {
            TargetOffset = baseGhost.targetOffset.ToDto()
        };
        return metadata;
    }

    public static BaseDeconstructableGhostMetadata GetBaseDeconstructableMetadata(this BaseGhost baseGhost)
    {
        BaseDeconstructableGhostMetadata metadata = baseGhost.GetMetadata<BaseDeconstructableGhostMetadata>();
        if (baseGhost.TryGetComponentInParent(out ConstructableBase constructableBase, true) && constructableBase.moduleFace.HasValue)
        {
            Base.Face moduleFace = constructableBase.moduleFace.Value;
            metadata.ModuleFace = moduleFace.ToDto();
            moduleFace.cell += baseGhost.targetBase.GetAnchor();
            Component baseModule = baseGhost.targetBase.GetModule(moduleFace).AliveOrNull();
            if (baseModule && baseModule.TryGetComponent(out PrefabIdentifier identifier))
            {
                metadata.ClassId = identifier.ClassId;
            }
        }

        return metadata;
    }

    public static BaseAnchoredFaceGhostMetadata GetBaseAnchoredFaceMetadata(this BaseGhost baseGhost)
    {
        BaseAnchoredFaceGhostMetadata metadata = baseGhost.GetMetadata<BaseAnchoredFaceGhostMetadata>();
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

    public static BaseAnchoredCellGhostMetadata GetBaseAnchoredCellMetadata(this BaseGhost baseGhost)
    {
        BaseAnchoredCellGhostMetadata metadata = baseGhost.GetMetadata<BaseAnchoredCellGhostMetadata>();
        if (baseGhost is BaseAddPartitionGhost ghost && ghost.anchoredCell.HasValue)
        {
            metadata.AnchoredCell = ghost.anchoredCell.Value.ToDto();
        }

        return metadata;
    }
}
