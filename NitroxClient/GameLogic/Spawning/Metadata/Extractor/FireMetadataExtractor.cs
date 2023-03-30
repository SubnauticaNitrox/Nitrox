using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class FireMetadataExtractor : GenericEntityMetadataExtractor<Fire, FireMetadata>
{
    public override FireMetadata Extract(Fire entity)
    {
        return new(entity.livemixin.health);
    }
}
