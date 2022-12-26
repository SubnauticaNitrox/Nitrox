using NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Bases.New;

public static class NitroxGhostMetadata
{
    public static T From<T>(BaseGhost baseGhost) where T : NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata.GhostMetadata, new()
    {
        T metadata = new();
        metadata.TargetOffset = baseGhost.targetOffset.ToDto();
        return metadata;
    }

    public static void ApplyTo(this NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata.GhostMetadata ghostMetadata, BaseGhost baseGhost)
    {
        baseGhost.targetOffset = ghostMetadata.TargetOffset.ToUnity();
    }

    public static NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata.GhostMetadata GetMetadataForGhost(BaseGhost baseGhost)
    {
        NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata.GhostMetadata metadata = baseGhost switch
        {
            BaseAddWaterPark or BaseAddPartitionDoorGhost or BaseAddModuleGhost or BaseAddFaceGhost => NitroxBaseAnchoredFaceGhostMetadata.From(baseGhost),
            BaseAddPartitionGhost => NitroxBaseAnchoredCellGhostMetadata.From(baseGhost),
            _ => From<NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata.GhostMetadata>(baseGhost),
        };
        return metadata;
    }

    public static void ApplyMetadataToGhost(BaseGhost baseGhost, NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata.GhostMetadata ghostMetadata)
    {
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
}

public static class NitroxBaseAnchoredFaceGhostMetadata
{
    public static BaseAnchoredFaceGhostMetadata From(BaseGhost baseGhost)
    {
        BaseAnchoredFaceGhostMetadata metadata = NitroxGhostMetadata.From<BaseAnchoredFaceGhostMetadata>(baseGhost);
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

    public static void ApplyTo(this BaseAnchoredFaceGhostMetadata ghostMetadata, BaseGhost baseGhost)
    {
        NitroxGhostMetadata.ApplyTo(ghostMetadata, baseGhost);
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
}

public static class NitroxBaseAnchoredCellGhostMetadata
{
    public static BaseAnchoredCellGhostMetadata From(BaseGhost baseGhost)
    {
        BaseAnchoredCellGhostMetadata metadata = NitroxGhostMetadata.From<BaseAnchoredCellGhostMetadata>(baseGhost);
        if (baseGhost is BaseAddPartitionGhost ghost && ghost.anchoredCell.HasValue)
        {
            metadata.AnchoredCell = ghost.anchoredCell.Value.ToDto();
        }
        return metadata;
    }

    public static void ApplyTo(this BaseAnchoredCellGhostMetadata ghostMetadata, BaseGhost baseGhost)
    {
        NitroxGhostMetadata.ApplyTo(ghostMetadata, baseGhost);
        if (ghostMetadata.AnchoredCell.HasValue)
        {
            if (baseGhost is BaseAddPartitionGhost ghost)
            {
                ghost.anchoredCell = ghostMetadata.AnchoredCell.Value.ToUnity();
            }
        }
    }
}
