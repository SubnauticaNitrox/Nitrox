using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.DataStructures.Util;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

public interface IEntityMetadataExtractor
{
    public Optional<EntityMetadata> From(object o);
}
