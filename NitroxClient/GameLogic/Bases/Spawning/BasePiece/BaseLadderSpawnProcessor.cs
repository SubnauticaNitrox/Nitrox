using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning.BasePiece
{
    public class BaseLadderSpawnProcessor : BasePieceSpawnProcessor
    {
        protected override TechType[] ApplicableTechTypes { get; } =
        {
            TechType.BaseLadder
        };

        protected override void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece)
        {
            bool builtLadderOnFloor = finishedPiece.name.Contains("Bottom");
            int searchDirection = builtLadderOnFloor ? -1 : 1;
            bool shouldSearch = true;

            Int3 cellToSearch;
            Optional<GameObject> otherLadderPiece;
            int searchOffset = searchDirection;
            do
            {
                cellToSearch = new Int3(latestCell.x, latestCell.y + searchOffset, latestCell.z);

                otherLadderPiece = FindSecondLadderPiece(latestBase, cellToSearch, out bool shouldKeepSearching);
                if (otherLadderPiece.HasValue || !shouldKeepSearching)
                {
                    shouldSearch = false;
                }
                searchOffset += searchDirection;
            } while (shouldSearch);

            if (otherLadderPiece.HasValue)
            {
                // Ladders are one of the rare instances where we want to assign the same id to two different objects.
                // This happens because the ladder can be deconstructed from two locations (the top and bottom).
                NitroxId id = NitroxEntity.GetId(finishedPiece);
                NitroxEntity.SetNewId(otherLadderPiece.Value, id);
                Log.Debug($"Successfully set new id to other piece: {otherLadderPiece.Value.name}, id={id}");
            }
            else
            {
                Log.Error($"Could not locate other ladder piece when searching cells : {cellToSearch}, builtLadderOnFloor: {builtLadderOnFloor}");
            }
        }

        private Optional<GameObject> FindSecondLadderPiece(Base latestBase, Int3 cellToSearch, out bool shouldKeepSearching)
        {
            Transform cellTransform = latestBase.GetCellObject(cellToSearch);
            shouldKeepSearching = false;
            if (!cellTransform)
            {
                return Optional.Empty;
            }
            foreach (Transform child in cellTransform)
            {
                NitroxEntity id = child.GetComponent<NitroxEntity>();
                BaseDeconstructable baseDeconstructable = child.GetComponent<BaseDeconstructable>();
                bool isNewBasePiece = id == null && baseDeconstructable != null;
                
                if (isNewBasePiece)
                {
                    TechType techType = baseDeconstructable.recipe;
                    if (techType == TechType.BaseLadder)
                    {
                        return Optional.Of(child.gameObject);
                    }
                }
                if (child.name.Contains("ConnectorLadder"))
                {
                    shouldKeepSearching = true;
                }
            }

            return Optional.Empty;
        }
    }
}
