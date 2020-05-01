using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public abstract class EntityMetadataProcessor
    {
        public abstract void ProcessMetadata(GameObject gameObject, EntityMetadata metadata);
        
        private static Dictionary<Type, Optional<EntityMetadataProcessor>> processorsByType = new Dictionary<Type, Optional<EntityMetadataProcessor>>();

        static EntityMetadataProcessor()
        {
            IEnumerable<EntityMetadataProcessor> processors = Assembly.GetExecutingAssembly()
                                                                         .GetTypes()
                                                                         .Where(t => typeof(EntityMetadataProcessor).IsAssignableFrom(t) &&
                                                                                     t.IsClass && 
                                                                                     !t.IsAbstract
                                                                               )
                                                                         .Select(Activator.CreateInstance)
                                                                         .Cast<EntityMetadataProcessor>();

            foreach (EntityMetadataProcessor processor in processors)
            {                
                Type metadataType = processor.GetType().BaseType.GetGenericArguments()[0];
                processorsByType.Add(metadataType, Optional.Of(processor));
            }
        }

        public static Optional<EntityMetadataProcessor> FromMetaData(EntityMetadata metadata)
        {
            Optional<EntityMetadataProcessor> processor;

            if (metadata != null && processorsByType.TryGetValue(metadata.GetType(), out processor))
            {
                return processor;
            }

            return Optional.Empty;
        }
    }
}
