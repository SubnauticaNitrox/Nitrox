using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class ExosuitMetadataExtractor : EntityMetadataExtractor<Exosuit, ExosuitMetadata>
{
    public override ExosuitMetadata Extract(Exosuit exosuit)
    {
        LiveMixin liveMixin = exosuit.liveMixin;
        SubName subName = exosuit.subName;

        return new(liveMixin.health, exosuit.precursorOutOfWater, SubNameInputMetadataExtractor.GetName(subName), SubNameInputMetadataExtractor.GetColors(subName));
    }
}
