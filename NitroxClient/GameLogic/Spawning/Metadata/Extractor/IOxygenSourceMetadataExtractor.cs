using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class IOxygenSourceMetadataExtractor : EntityMetadataExtractor<IOxygenSource, IOxygenSourceMetadata>
{
    public override IOxygenSourceMetadata Extract(IOxygenSource entity)
    {
        return new IOxygenSourceMetadata(entity.GetOxygenAvailable());
    }
}
