using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class SealedDoorMetadataExtractor : EntityMetadataExtractor<Sealed, SealedDoorMetadata>
{
    public override SealedDoorMetadata Extract(Sealed entity)
    {
        return new(entity._sealed, entity.openedAmount);
    }
}
