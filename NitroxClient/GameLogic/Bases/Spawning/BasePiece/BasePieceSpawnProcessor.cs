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
            if (finishedPiece.TryGetComponent(out NitroxEntity nitroxEntity))
            {
                spawnProcessorsApplied[nitroxEntity.Id] = new BasePieceSpawnInfos(techType, latestBase, latestCell);
            }
            RunSpawnProcessor(techType, latestBase, latestCell, finishedPiece);
        }

        private static void RunSpawnProcessor(TechType techType, Base latestBase, Int3 latestCell, GameObject finishedPiece)
        {
            if (processorsByType.TryGetValue(techType, out BasePieceSpawnProcessor processor))
            {
                Log.Info($"Found custom BasePieceSpawnProcessor for {techType}");
                processor.SpawnPostProcess(latestBase, latestCell, finishedPiece);
            }
        }

        public static void ReRunSpawnProcessor(GameObject finishedPiece, NitroxId nitroxId)
        {
            if (spawnProcessorsApplied.TryGetValue(nitroxId, out BasePieceSpawnInfos basePieceSpawnInfos))
            {
                RunSpawnProcessor(basePieceSpawnInfos.TechType, basePieceSpawnInfos.LatestBase, basePieceSpawnInfos.LatestCell, finishedPiece);
            }
        }

        class BasePieceSpawnInfos
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
