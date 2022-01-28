using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
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
            
            Int3 cellToSearch = Int3.zero;
            Optional<GameObject> otherLadderPiece = Optional.Empty;
            int searchOffset = searchDirection;
            int maxY = NitroxServiceLocator.LocateService<IMap>().DimensionsInBatches.Y;
            for (int i = 0; i < maxY; i++)
            {
                cellToSearch = new Int3(latestCell.x, latestCell.y + searchOffset, latestCell.z);

                if (!FindSecondLadderPiece(latestBase, cellToSearch, out otherLadderPiece) || otherLadderPiece.HasValue)
                {
                    break;
                }
                searchOffset += searchDirection;
            }

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

        private bool FindSecondLadderPiece(Base latestBase, Int3 cellToSearch, out Optional<GameObject> piece)
        {
            Transform cellTransform = latestBase.GetCellObject(cellToSearch);
            piece = Optional.Empty;
            if (!cellTransform)
            {
                return false;
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
                        piece = Optional.Of(child.gameObject);
                        return true;
                    }
                }
                if (child.name.Contains("ConnectorLadder"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
