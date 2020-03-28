using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning
{
    public class BaseLadderSpawnProcessor : BasePieceSpawnProcessor
    {
        public override TechType[] ApplicableTechTypes { get; } =
        {
            TechType.BaseLadder
        };

        public override void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece)
        {
            bool builtLadderOnFloor = finishedPiece.name.Contains("Bottom");
            Int3 cellToSearch = builtLadderOnFloor ? new Int3(latestCell.x, latestCell.y - 1, latestCell.z) : new Int3(latestCell.x, latestCell.y + 1, latestCell.z);

            Optional<GameObject> otherLadderPiece = FindSecondLadderPiece(latestBase, cellToSearch);

            if (otherLadderPiece.HasValue)
            {
                // Ladders are one of the rare instances where we want to assign the same id to two different objects.
                // This happens because the ladder can be deconstructed from two locations (the top and bottom).
                NitroxId id = NitroxEntity.GetId(finishedPiece);
                NitroxEntity.SetNewId(otherLadderPiece.Value, id);
            }
            else
            {
                Log.Info("Could not locate other ladder piece when searching cell: " + cellToSearch + " builtLadderOnFloor: " + builtLadderOnFloor);
            }
        }

        private Optional<GameObject> FindSecondLadderPiece(Base latestBase, Int3 cellToSearch)
        {
            Transform cellTransform = latestBase.GetCellObject(cellToSearch);

            foreach (Transform child in cellTransform)
            {
                NitroxEntity id = child.GetComponent<NitroxEntity>();
                BaseDeconstructable baseDeconstructable = child.GetComponent<BaseDeconstructable>();

                bool isNewBasePiece = id == null && baseDeconstructable != null;

                if (isNewBasePiece)
                {
                    TechType techType = (TechType)baseDeconstructable.ReflectionGet("recipe");

                    if (techType == TechType.BaseLadder)
                    {
                        return Optional.Of(child.gameObject);
                    }
                }
            }

            return Optional.Empty;
        }
    }
}
