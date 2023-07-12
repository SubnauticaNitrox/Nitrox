using NitroxModel.DataStructures.GameLogic.Bases.Metadata;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Bases.MetadataUtils;

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
