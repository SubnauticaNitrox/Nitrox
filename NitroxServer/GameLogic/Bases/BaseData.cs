using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Bases
{
    public class BaseData
    {
        public Dictionary<string, BasePiece> BasePiecesByGuid { get; set; } = new Dictionary<string, BasePiece>();
        public List<BasePiece> CompletedBasePieceHistory { get; set; } = new List<BasePiece>();

        public void AddBasePiece(BasePiece basePiece)
        {
            BasePiecesByGuid.Add(basePiece.Guid, basePiece);
        }

        public void BasePieceConstructionAmountChanged(string guid, float constructionAmount)
        {
            BasePiecesByGuid[guid].ConstructionAmount = constructionAmount;
        }

        public void BasePieceConstructionCompleted(string guid, Optional<string> newlyCreatedParentGuid)
        {
            BasePiece basePiece = BasePiecesByGuid[guid];

            basePiece.ConstructionAmount = 1.0f;
            basePiece.ConstructionCompleted = true;
            basePiece.NewBaseGuid = newlyCreatedParentGuid;

            if(newlyCreatedParentGuid.IsPresent())
            {
                basePiece.ParentGuid = Optional<string>.Empty();
            }

            CompletedBasePieceHistory.Add(basePiece);
        }

        public void BasePieceDeconstructionBegin(string guid)
        {
            BasePiece basePiece = BasePiecesByGuid[guid];

            basePiece.ConstructionAmount = 0.95f;
            basePiece.ConstructionCompleted = false;
        }

        public void BasePieceDeconstructionCompleted(string guid)
        {
            BasePiece basePiece = BasePiecesByGuid[guid];
            CompletedBasePieceHistory.Remove(basePiece);
            BasePiecesByGuid.Remove(guid);
        }

        public List<BasePiece> GetBasePiecesForNewlyConnectedPlayer()
        {
            // Play back all completed base pieces first (other pieces have a dependency on these being done)
            List<BasePiece> basePieces = new List<BasePiece>(CompletedBasePieceHistory);

            // Play back pieces that may not be completed yet.
            foreach(BasePiece basePiece in BasePiecesByGuid.Values)
            {
                if(!basePieces.Contains(basePiece))
                {
                    basePieces.Add(basePiece);
                }
            }

            return basePieces;
        }
    }
}
