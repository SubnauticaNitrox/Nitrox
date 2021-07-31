using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.GameLogic.Bases
{
    public class BaseManager
    {
        private Dictionary<NitroxId, BasePiece> partiallyConstructedPiecesById = new Dictionary<NitroxId, BasePiece>();
        private List<BasePiece> completedBasePieceHistory;
        private int nextBuildIndex;

        public BaseManager(List<BasePiece> partiallyConstructedPieces, List<BasePiece> completedBasePieceHistory)
        {
            this.completedBasePieceHistory = completedBasePieceHistory;
            partiallyConstructedPiecesById = partiallyConstructedPieces.ToDictionary(piece => piece.Id);

            int highestPartialIndex = partiallyConstructedPieces.Any() ? partiallyConstructedPieces.Max(piece => piece.BuildIndex) : 0;
            int highestCompletedIndex = completedBasePieceHistory.Any() ? completedBasePieceHistory.DefaultIfEmpty().Max(piece => piece.BuildIndex) : 0;

            nextBuildIndex = Math.Max(highestPartialIndex, highestCompletedIndex) + 1;
        }

        public List<BasePiece> GetCompletedBasePieceHistory()
        {
            lock (completedBasePieceHistory)
            {
                return new List<BasePiece>(completedBasePieceHistory);
            }
        }

        public List<BasePiece> GetPartiallyConstructedPieces()
        {
            lock (partiallyConstructedPiecesById)
            {
                return new List<BasePiece>(partiallyConstructedPiecesById.Values);
            }
        }

        public void AddBasePiece(BasePiece basePiece)
        {
            lock (partiallyConstructedPiecesById)
            {
                basePiece.BuildIndex = ++nextBuildIndex;
                partiallyConstructedPiecesById.Add(basePiece.Id, basePiece);
            }
        }

        public void BasePieceConstructionAmountChanged(NitroxId id, float constructionAmount)
        {

            lock (partiallyConstructedPiecesById)
            {
                if (partiallyConstructedPiecesById.TryGetValue(id, out BasePiece basePiece))
                {
                    basePiece.ConstructionAmount = constructionAmount;

                    if (basePiece.ConstructionCompleted)
                    {
                        basePiece.ConstructionCompleted = false;
                    }
                }
            }
        }

        public void BasePieceConstructionCompleted(NitroxId id, NitroxId baseId)
        {

            lock (partiallyConstructedPiecesById)
            {
                if (partiallyConstructedPiecesById.TryGetValue(id, out BasePiece basePiece))
                {
                    basePiece.ConstructionAmount = 1.0f;
                    basePiece.ConstructionCompleted = true;

                    if (!basePiece.IsFurniture)
                    {
                        // For standard base pieces, the baseId is may not be finialized until construction 
                        // completes because Subnautica uses a GhostBase in the world if there hasn't yet been
                        // a fully constructed piece.  Therefor, we always update this attribute to make sure it
                        // is the latest.
                        basePiece.BaseId = baseId;
                        basePiece.ParentId = Optional.OfNullable(baseId);
                    }

                    partiallyConstructedPiecesById.Remove(id);

                    lock (completedBasePieceHistory)
                    {
                        completedBasePieceHistory.Add(basePiece);
                    }
                }
            }
        }

        public void BasePieceDeconstructionBegin(NitroxId id)
        {
            BasePiece basePiece;

            lock (completedBasePieceHistory)
            {
                basePiece = completedBasePieceHistory.Find(piece => piece.Id == id);

                if (basePiece != null)
                {
                    basePiece.ConstructionAmount = 0.95f;
                    basePiece.ConstructionCompleted = false;
                    completedBasePieceHistory.Remove(basePiece);

                    lock (partiallyConstructedPiecesById)
                    {
                        partiallyConstructedPiecesById[basePiece.Id] = basePiece;
                    }
                }
            }
        }

        public void BasePieceDeconstructionCompleted(NitroxId id)
        {
            lock (partiallyConstructedPiecesById)
            {
                partiallyConstructedPiecesById.Remove(id);
            }
        }

        public void UpdateBasePieceMetadata(NitroxId id, BasePieceMetadata metadata)
        {
            BasePiece basePiece;

            lock (completedBasePieceHistory)
            {
                basePiece = completedBasePieceHistory.Find(piece => piece.Id == id);

                if (basePiece != null)
                {
                    basePiece.Metadata = Optional.OfNullable(metadata);
                }
            }
        }

        public List<BasePiece> GetBasePiecesForNewlyConnectedPlayer()
        {
            List<BasePiece> basePieces;

            lock (completedBasePieceHistory)
            {
                // Play back all completed base pieces first (other pieces have a dependency on these being done)
                basePieces = new List<BasePiece>(completedBasePieceHistory);
            }

            lock (partiallyConstructedPiecesById)
            {
                // Play back pieces that may not be completed yet.
                foreach (BasePiece partialBasePiece in partiallyConstructedPiecesById.Values)
                {
                    if (!basePieces.Contains(partialBasePiece))
                    {
                        basePieces.Add(partialBasePiece);
                    }
                }
            }

            return basePieces;
        }
    }
}
