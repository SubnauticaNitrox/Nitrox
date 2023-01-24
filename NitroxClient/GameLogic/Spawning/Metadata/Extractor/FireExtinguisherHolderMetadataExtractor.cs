using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class FireExtinguisherHolderMetadataExtractor : GenericEntityMetadataExtractor<FireExtinguisherHolder, FireExtinguisherHolderMetadata>
{
    public override FireExtinguisherHolderMetadata Extract(FireExtinguisherHolder entity)
    {
        return new(entity.hasTank, entity.fuel);
    }
}
