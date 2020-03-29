using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning
{
    public class NoOpBasePieceSpawnProcessor : BasePieceSpawnProcessor
    {
        public override TechType[] ApplicableTechTypes { get; } = new TechType[0];

        public override void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece)
        {
            // No-Op!
        }
    }
}
