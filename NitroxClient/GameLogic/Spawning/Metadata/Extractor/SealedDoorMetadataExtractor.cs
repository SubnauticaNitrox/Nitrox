using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class SealedDoorMetadataExtractor : EntityMetadataExtractor<Sealed, SealedDoorMetadata>
{
    public override SealedDoorMetadata Extract(Sealed entity)
    {
        return new(entity._sealed, entity.openedAmount);
    }
}
