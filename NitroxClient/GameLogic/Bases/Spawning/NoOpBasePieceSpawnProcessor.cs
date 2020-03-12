using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning
{
    public class NoOpBasePieceSpawnProcessor : BasePieceSpawnProcessor
    {
        public override void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece)
        {
            // No-Op!
        }

        public override List<TechType> GetApplicableTechTypes()
        {
            return new List<TechType>();
        }
    }
}
