using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class ConstructorMetadataExtractor : GenericEntityMetadataExtractor<Constructor, ConstructorMetadata>
{
    public override ConstructorMetadata Extract(Constructor entity)
    {
        return new(entity.deployed);
    }
}
