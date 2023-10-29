using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class ExosuitMetadataExtractor : VehicleMetadataExtractor<Exosuit, ExosuitMetadata>
{
    public override ExosuitMetadata Extract(Exosuit exosuit)
    {
        LiveMixin liveMixin = exosuit.liveMixin;
        SubName subName = exosuit.subName;

        return new(GetHealth(liveMixin), GetName(subName), GetColors(subName));
    }
}
