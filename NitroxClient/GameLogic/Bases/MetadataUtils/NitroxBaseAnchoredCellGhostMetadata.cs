using NitroxModel.DataStructures.GameLogic.Bases.Metadata;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Bases.MetadataUtils;

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
