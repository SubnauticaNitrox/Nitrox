using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

public abstract class EntityMetadataExtractor<I, O> : IEntityMetadataExtractor where O : EntityMetadata
{
    public abstract O Extract(I entity);

    public Optional<EntityMetadata> From(object o)
    {
        EntityMetadata result = Extract((I)o);

        return Optional.OfNullable(result);
    }
}
