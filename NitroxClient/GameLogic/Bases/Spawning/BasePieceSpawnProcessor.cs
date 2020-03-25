using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning
{
    public abstract class BasePieceSpawnProcessor
    {
        private static readonly NoOpBasePieceSpawnProcessor noOpProcessor = new NoOpBasePieceSpawnProcessor();
        private static readonly Dictionary<TechType, BasePieceSpawnProcessor> processorsByType = new Dictionary<TechType, BasePieceSpawnProcessor>();

        public abstract TechType[] ApplicableTechTypes { get; }

        static BasePieceSpawnProcessor()
        {
            IEnumerable<BasePieceSpawnProcessor> processors = Assembly.GetExecutingAssembly()
                                                                         .GetTypes()
                                                                         .Where(t => typeof(BasePieceSpawnProcessor).IsAssignableFrom(t) &&
                                                                                     t.IsClass && !t.IsAbstract && t != typeof(NoOpBasePieceSpawnProcessor)
                                                                               )
                                                                         .Select(Activator.CreateInstance)
                                                                         .Cast<BasePieceSpawnProcessor>();

            foreach (BasePieceSpawnProcessor processor in processors)
            {
                foreach (TechType techType in processor.ApplicableTechTypes)
                {
                    processorsByType.Add(techType, processor);
                }
            }
        }

        public abstract void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece);

        public static BasePieceSpawnProcessor From(BaseDeconstructable baseDeconstructable)
        {
            TechType techType = (TechType)baseDeconstructable.ReflectionGet("recipe");

            BasePieceSpawnProcessor processor;

            if (processorsByType.TryGetValue(techType, out processor))
            {
                Log.Info("Found custom BasePieceSpawnProcessor for " + techType);
                return processor;
            }

            return noOpProcessor;
        }
    }
}
