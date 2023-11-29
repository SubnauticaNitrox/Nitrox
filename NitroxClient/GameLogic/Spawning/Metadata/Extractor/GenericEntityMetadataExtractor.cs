using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public abstract class GenericEntityMetadataExtractor<I, O> : EntityMetadataExtractor where O : EntityMetadata
{
    public abstract O Extract(I entity);

    public override Optional<EntityMetadata> From(object o)
    {
        EntityMetadata result = Extract((I)o);

        return Optional.OfNullable(result);
    }
}

public abstract class EntityMetadataExtractor
{
    public abstract Optional<EntityMetadata> From(object o);

    private static readonly Dictionary<Type, EntityMetadataExtractor> processors;

    static EntityMetadataExtractor()
    {
        processors = NitroxServiceLocator.LocateService<IEnumerable<EntityMetadataExtractor>>()
                                         .ToDictionary(p => p.GetType().BaseType.GetGenericArguments()[0]);    
    }

    public static Optional<EntityMetadata> Extract(object o)
    {
        if (processors.TryGetValue(o.GetType(), out EntityMetadataExtractor extractor))
        {
            return extractor.From(o);
        }

        return Optional.Empty;
    }

    public static Optional<EntityMetadata> Extract(GameObject o)
    {
        foreach (Component component in o.GetComponents<Component>())
        {
            if (processors.TryGetValue(component.GetType(), out EntityMetadataExtractor extractor))
            {
                return extractor.From(component);
            }
        }

        return Optional.Empty;
    }

}
