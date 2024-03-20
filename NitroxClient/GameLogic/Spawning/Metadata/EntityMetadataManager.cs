using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class EntityMetadataManager
{
    private readonly Dictionary<Type, IEntityMetadataExtractor> extractors;
    private readonly Dictionary<Type, IEntityMetadataProcessor> processors;

    /// <summary>
    /// As it takes some time spawning lots of entities, it can happen that we receive metadata updates for those which are still in the queue.
    /// Therefore we list the updates which happened while we didn't have the said objects loaded
    /// </summary>
    private readonly Dictionary<NitroxId, EntityMetadata> newerMetadataById = new();

    public EntityMetadataManager(IEnumerable<IEntityMetadataExtractor> extractors, IEnumerable<IEntityMetadataProcessor> processors)
    {
        this.extractors = extractors.ToDictionary(p => p.GetType().BaseType.GetGenericArguments()[0]);
        this.processors = processors.ToDictionary(p => p.GetType().BaseType.GetGenericArguments()[0]);
    }

    public Optional<EntityMetadata> Extract(object o)
    {
        if (extractors.TryGetValue(o.GetType(), out IEntityMetadataExtractor extractor))
        {
            return extractor.From(o);
        }

        return Optional.Empty;
    }

    public Optional<EntityMetadata> Extract(GameObject o)
    {
        foreach (Component component in o.GetComponents<Component>())
        {
            if (extractors.TryGetValue(component.GetType(), out IEntityMetadataExtractor extractor))
            {
                Optional<EntityMetadata> metadata = extractor.From(component);
                if (metadata.HasValue)
                {
                    return metadata;
                }
            }
        }

        return Optional.Empty;
    }

    public Optional<IEntityMetadataProcessor> FromMetaData(EntityMetadata metadata)
    {
        if (metadata != null && processors.TryGetValue(metadata.GetType(), out IEntityMetadataProcessor processor))
        {
            return Optional.Of(processor);
        }

        return Optional.Empty;
    }

    public void ApplyMetadata(GameObject gameObject, EntityMetadata metadata)
    {
        // In case a metadata update was received while this entity was in the spawn queue
        if (gameObject.TryGetNitroxId(out NitroxId objectId) &&
            newerMetadataById.TryGetValue(objectId, out EntityMetadata newMetadata))
        {
            metadata = newMetadata;
            newerMetadataById.Remove(objectId);
        }
        Optional<IEntityMetadataProcessor> metadataProcessor = FromMetaData(metadata);

        if (metadataProcessor.HasValue)
        {
            metadataProcessor.Value.ProcessMetadata(gameObject, metadata);
        }
    }

    public void RegisterNewerMetadata(NitroxId entityId, EntityMetadata metadata)
    {
        newerMetadataById[entityId] = metadata;
    }

    public void ClearNewerMetadata()
    {
        newerMetadataById.Clear();
    }
}
