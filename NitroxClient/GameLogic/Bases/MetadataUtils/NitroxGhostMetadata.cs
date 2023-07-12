using System.Collections;
using NitroxModel.DataStructures.GameLogic.Bases.Metadata;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Bases.MetadataUtils;

public static class NitroxGhostMetadata
{
    public static T From<T>(BaseGhost baseGhost) where T : GhostMetadata, new()
    {
        T metadata = new()
        {
            TargetOffset = baseGhost.targetOffset.ToDto()
        };
        return metadata;
    }

    public static void ApplyTo(this GhostMetadata ghostMetadata, BaseGhost baseGhost)
    {
        baseGhost.targetOffset = ghostMetadata.TargetOffset.ToUnity();
    }

    public static GhostMetadata GetMetadataForGhost(BaseGhost baseGhost)
    {
        // Specific case in which a piece was deconstructed and resulted in a BaseDeconstructable with a normal BaseGhost
        if (BuildUtils.IsUnderBaseDeconstructable(baseGhost))
        {
            return NitroxBaseDeconstructableGhostMetadata.From(baseGhost);
        }

        GhostMetadata metadata = baseGhost switch
        {
            BaseAddWaterPark or BaseAddPartitionDoorGhost or BaseAddModuleGhost or BaseAddFaceGhost => NitroxBaseAnchoredFaceGhostMetadata.From(baseGhost),
            BaseAddPartitionGhost => NitroxBaseAnchoredCellGhostMetadata.From(baseGhost),
            _ => From<BasicGhostMetadata>(baseGhost),
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
            yield return deconstructableMetadata.ApplyTo(baseGhost, @base);
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
                    faceMetadata.ApplyTo(baseGhost);
                }
                break;
            case BaseAddPartitionGhost:
                if (ghostMetadata is BaseAnchoredCellGhostMetadata cellMetadata)
                {
                    cellMetadata.ApplyTo(baseGhost);
                }
                break;
            default:
                ghostMetadata.ApplyTo(baseGhost);
                break;
        }
    }

    public static void LateApplyMetadataToGhost(BaseGhost baseGhost, EntityMetadata entityMetadata)
    {
        if (BuildUtils.IsUnderBaseDeconstructable(baseGhost, true) && entityMetadata is BaseDeconstructableGhostMetadata deconstructableMetadata)
        {
            deconstructableMetadata.LateApplyTo(baseGhost);
        }
    }
}
