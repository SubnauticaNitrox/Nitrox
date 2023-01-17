using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public abstract class EntityMetadataProcessor
    {
        public abstract void ProcessMetadata(GameObject gameObject, EntityMetadata metadata);

        private static readonly Dictionary<Type, Optional<EntityMetadataProcessor>> processorsByType;

        static EntityMetadataProcessor()
        {
            processorsByType = NitroxServiceLocator.LocateService<IEnumerable<EntityMetadataProcessor>>()
                                                   .Select(x => Optional.Of(x))
                                                   .ToDictionary(p => p.Value.GetType().BaseType.GetGenericArguments()[0]);
        }

        public static Optional<EntityMetadataProcessor> FromMetaData(EntityMetadata metadata)
        {

            if (metadata != null && processorsByType.TryGetValue(metadata.GetType(), out Optional<EntityMetadataProcessor> processor))
            {
                return processor;
            }

            return Optional.Empty;
        }

        public static void ApplyMetadata(GameObject gameObject, EntityMetadata metadata)
        {
            Optional<EntityMetadataProcessor> metadataProcessor = EntityMetadataProcessor.FromMetaData(metadata);

            if (metadataProcessor.HasValue)
            {
                metadataProcessor.Value.ProcessMetadata(gameObject, metadata);
            }
        }
    }
}
