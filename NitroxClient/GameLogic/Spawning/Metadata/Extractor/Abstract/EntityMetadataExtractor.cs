using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.DataStructures.Util;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

public abstract class EntityMetadataExtractor<I, O> : IEntityMetadataExtractor where O : EntityMetadata
{
    public abstract O Extract(I entity);

    public Optional<EntityMetadata> From(object o)
    {
        EntityMetadata result = Extract((I)o);

        return Optional.OfNullable(result);
    }

    protected T Resolve<T>() where T : class
    {
        return NitroxServiceLocator.Cache<T>.Value;
    }
}
