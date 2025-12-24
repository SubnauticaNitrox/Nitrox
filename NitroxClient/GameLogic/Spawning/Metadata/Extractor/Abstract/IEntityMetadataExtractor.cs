using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

public interface IEntityMetadataExtractor
{
    public Optional<EntityMetadata> From(object o);
}
