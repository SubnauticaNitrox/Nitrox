using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class FireExtinguisherHolderMetadataExtractor : EntityMetadataExtractor<FireExtinguisherHolder, FireExtinguisherHolderMetadata>
{
    public override FireExtinguisherHolderMetadata Extract(FireExtinguisherHolder entity)
    {
        return new(entity.hasTank, entity.fuel);
    }
}
