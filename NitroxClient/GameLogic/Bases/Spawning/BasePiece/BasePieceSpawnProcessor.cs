using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning.BasePiece
{
    public abstract class BasePieceSpawnProcessor
    {
        private static readonly Dictionary<TechType, BasePieceSpawnProcessor> processorsByType = new Dictionary<TechType, BasePieceSpawnProcessor>();

        protected abstract TechType[] ApplicableTechTypes { get; }

        static BasePieceSpawnProcessor()
        {
            IEnumerable<BasePieceSpawnProcessor> processors = Assembly.GetExecutingAssembly()
                                                                         .GetTypes()
                                                                         .Where(t => typeof(BasePieceSpawnProcessor).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
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

        protected abstract void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece, bool justConstructed);

        public static void RunSpawnProcessor(BaseDeconstructable baseDeconstructable, Base latestBase, Int3 latestCell, GameObject finishedPiece, bool justConstructed)
        {
            TechType techType = baseDeconstructable.recipe;
            if (processorsByType.TryGetValue(techType, out BasePieceSpawnProcessor processor))
            {
                Log.Info($"Found custom BasePieceSpawnProcessor for {techType}");
                processor.SpawnPostProcess(latestBase, latestCell, finishedPiece, justConstructed);
            }
        }
    }
}
