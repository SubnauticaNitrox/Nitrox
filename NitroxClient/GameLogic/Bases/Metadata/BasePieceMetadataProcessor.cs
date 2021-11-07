using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;

namespace NitroxClient.GameLogic.Bases.Metadata
{
    public abstract class BasePieceMetadataProcessor
    {
        public abstract void UpdateMetadata(NitroxId id, BasePieceMetadata metadata, bool initialSync = false);

        private static NoOpBasePieceMetadataProcessor noOpProcessor = new NoOpBasePieceMetadataProcessor();
        private static Dictionary<Type, BasePieceMetadataProcessor> processorsByType = new Dictionary<Type, BasePieceMetadataProcessor>();

        static BasePieceMetadataProcessor()
        {
            IEnumerable<BasePieceMetadataProcessor> processors = Assembly.GetExecutingAssembly()
                                                                         .GetTypes()
                                                                         .Where(t => typeof(BasePieceMetadataProcessor).IsAssignableFrom(t) &&
                                                                                     t.IsClass && !t.IsAbstract && t != typeof(NoOpBasePieceMetadataProcessor)
                                                                               )
                                                                         .Select(Activator.CreateInstance)
                                                                        .Cast<BasePieceMetadataProcessor>();

            foreach (BasePieceMetadataProcessor processor in processors)
            {

                Type metadataType = processor.GetType().BaseType.GetGenericArguments()[0];
                processorsByType.Add(metadataType, processor);
            }
        }

        public static BasePieceMetadataProcessor FromMetaData(BasePieceMetadata metadata)
        {

            if (processorsByType.TryGetValue(metadata.GetType(), out BasePieceMetadataProcessor processor))
            {
                return processor;
            }

            return noOpProcessor;
        }
    }
}
