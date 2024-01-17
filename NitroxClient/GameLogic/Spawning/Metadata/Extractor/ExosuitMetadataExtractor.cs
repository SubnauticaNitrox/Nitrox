using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class ExosuitMetadataExtractor : EntityMetadataExtractor<Exosuit, ExosuitMetadata>
{
    public override ExosuitMetadata Extract(Exosuit exosuit)
    {
        LiveMixin liveMixin = exosuit.liveMixin;
        SubName subName = exosuit.subName;

        return new(liveMixin.health, SubNameInputMetadataExtractor.GetName(subName), SubNameInputMetadataExtractor.GetColors(subName));
    }
}
