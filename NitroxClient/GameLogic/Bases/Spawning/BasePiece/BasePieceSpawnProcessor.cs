using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning.BasePiece
{
    public abstract class BasePieceSpawnProcessor
    {
        private static readonly Dictionary<TechType, BasePieceSpawnProcessor> processorsByType = new Dictionary<TechType, BasePieceSpawnProcessor>();
        private static readonly Dictionary<NitroxId, BasePieceSpawnInfos> spawnProcessorsApplied = new();

        protected abstract TechType[] ApplicableTechTypes { get; }
        /// <summary>
        /// Some processors don't need to be rerun because the modifications they apply aren't reset when base geometry is rebuilt
        /// </summary>
        protected abstract bool ShouldRerunSpawnProcessor { get; }

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

        protected abstract void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece);

        public static void RunSpawnProcessor(BaseDeconstructable baseDeconstructable, Base latestBase, Int3 latestCell, GameObject finishedPiece)
        {
            TechType techType = baseDeconstructable.recipe;
            
            if (RunSpawnProcessor(techType, latestBase, latestCell, finishedPiece))
            {
                if (finishedPiece.TryGetComponent(out NitroxEntity nitroxEntity))
                {
                    spawnProcessorsApplied[nitroxEntity.Id] = new BasePieceSpawnInfos(techType, latestBase, latestCell);
                }
            }
        }

        private static bool RunSpawnProcessor(TechType techType, Base latestBase, Int3 latestCell, GameObject finishedPiece, bool isReRun = false)
        {
            if (processorsByType.TryGetValue(techType, out BasePieceSpawnProcessor processor) && (!isReRun || processor.ShouldRerunSpawnProcessor))
            {
                Log.Info($"Found custom BasePieceSpawnProcessor for {techType}");
                processor.SpawnPostProcess(latestBase, latestCell, finishedPiece);
                return true;
            }
            return false;
        }

        // Each time an element is built in a base, all the geometries are rebuilt and therefore the modifications done in the SpawnProcessors are erased
        // Thus, we need to reapply them each time we rebuild the geometry
        public static void ReRunSpawnProcessor(GameObject finishedPiece, NitroxId nitroxId)
        {
            if (spawnProcessorsApplied.TryGetValue(nitroxId, out BasePieceSpawnInfos basePieceSpawnInfos))
            {
                RunSpawnProcessor(basePieceSpawnInfos.TechType, basePieceSpawnInfos.LatestBase, basePieceSpawnInfos.LatestCell, finishedPiece, true);
            }
        }

        internal struct BasePieceSpawnInfos
        {
            public TechType TechType;
            public Base LatestBase;
            public Int3 LatestCell;

            public BasePieceSpawnInfos(TechType techType, Base latestBase, Int3 latestCell)
            {
                TechType = techType;
                LatestBase = latestBase;
                LatestCell = latestCell;
            }
        }
    }
}
