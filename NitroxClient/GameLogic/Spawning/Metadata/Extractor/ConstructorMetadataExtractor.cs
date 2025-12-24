using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class ConstructorMetadataExtractor : EntityMetadataExtractor<Constructor, ConstructorMetadata>
{
    public override ConstructorMetadata Extract(Constructor entity)
    {
        return new(entity.deployed);
    }
}
