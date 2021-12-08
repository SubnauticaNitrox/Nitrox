using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning.Furniture
{
    public abstract class FurnitureSpawnProcessor
    {
        private static readonly Dictionary<TechType, FurnitureSpawnProcessor> processorsByType = new Dictionary<TechType, FurnitureSpawnProcessor>();

        protected abstract TechType[] ApplicableTechTypes { get; }

        static FurnitureSpawnProcessor()
        {
            IEnumerable<FurnitureSpawnProcessor> processors = Assembly.GetExecutingAssembly()
                                                                      .GetTypes()
                                                                      .Where(t => typeof(FurnitureSpawnProcessor).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                                                                      .Select(Activator.CreateInstance)
                                                                      .Cast<FurnitureSpawnProcessor>();

            foreach (FurnitureSpawnProcessor processor in processors)
            {
                foreach (TechType techType in processor.ApplicableTechTypes)
                {
                    processorsByType.Add(techType, processor);
                }
            }
        }

        protected abstract void SpawnPostProcess(GameObject finishedFurniture);

        public static void RunSpawnProcessor(Constructable constructable)
        {
            if (processorsByType.TryGetValue(constructable.techType, out FurnitureSpawnProcessor processor))
            {
                Log.Info($"Found custom FurnitureSpawnProcessor for {constructable.techType}");
                processor.SpawnPostProcess(constructable.gameObject);
            }
        }
    }
}
