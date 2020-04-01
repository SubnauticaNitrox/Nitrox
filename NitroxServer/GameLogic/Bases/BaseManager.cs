using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures;
using System.Linq;
using NitroxModel.DataStructures.Util;


namespace NitroxServer.GameLogic.Bases
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class BaseManager
    {
        private Dictionary<NitroxId, BasePiece> partiallyConstructedPiecesById = new Dictionary<NitroxId, BasePiece>();
        private List<BasePiece> completedBasePieceHistory;

        public BaseManager(List<BasePiece> partiallyConstructedPieces, List<BasePiece> completedBasePieceHistory)
        {
            this.completedBasePieceHistory = completedBasePieceHistory;
            partiallyConstructedPiecesById = partiallyConstructedPieces.ToDictionary(piece => piece.Id);
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
            //anything that is not a window, hatch, or reinforcement get run normally
            if (basePiece.TechType.Name != "BaseReinforcement" | basePiece.TechType.Name != "BaseHatch" | basePiece.TechType.Name != "BaseWindow")
            {
                //just in case some how a base piece is spawned at max completion
                //the base piece will get added to at least one directory
                if(basePiece.ConstructionAmount < 1.0f)
                {
                    lock (partiallyConstructedPiecesById)
                    {
                        partiallyConstructedPiecesById.Add(basePiece.Id, basePiece);
                    }
                }
                else
                {
                    lock (partiallyConstructedPiecesById)
                    {
                        completedBasePieceHistory.Add(basePiece);
                    }
                }
            }
            //when adding base peices this tries to eliminate partially constructed parts that were once fully constructed
            if (basePiece.ConstructionAmount > 0.4f && basePiece.ConstructionAmount != 1.0f)
            {
                basePiece.ConstructionAmount = 1.0f;
                basePiece.ConstructionCompleted = true;
            }
            //I am not sure if this helps at all, but it is supposed to ensure that the windows, hatches, and reinforcements don't go missing
            //and that they they are built to 100%
            if (basePiece.TechType.Name == "BaseWindow" && completedBasePieceHistory.Contains(basePiece))
            {
                basePiece.ConstructionCompleted = false;
                partiallyConstructedPiecesById.Add(basePiece.Id, basePiece);
                basePiece.ConstructionAmount = 1.0f;
                basePiece.ConstructionCompleted = true;
            }
            if (basePiece.TechType.Name == "BaseHatch" && completedBasePieceHistory.Contains(basePiece))
            {
                basePiece.ConstructionCompleted = false;
                partiallyConstructedPiecesById.Add(basePiece.Id, basePiece);
                basePiece.ConstructionAmount = 1.0f;
                basePiece.ConstructionCompleted = true;
            }
            if (basePiece.TechType.Name == "BaseReinforcement" && completedBasePieceHistory.Contains(basePiece))
            {
                basePiece.ConstructionCompleted = false;
                partiallyConstructedPiecesById.Add(basePiece.Id, basePiece);
                basePiece.ConstructionAmount = 1.0f;
                basePiece.ConstructionCompleted = true;
            }

        }

        public void BasePieceConstructionAmountChanged(NitroxId id, float constructionAmount)
        {
            BasePiece basePiece;
            lock (partiallyConstructedPiecesById)
            {
                if (partiallyConstructedPiecesById.TryGetValue(id, out basePiece))
                {
                    basePiece.ConstructionAmount = constructionAmount;
                    /*
                     // this if defaults to true, so if construction is completed, it gets set to flase so its not completed
                     // I could be wrong but this might cause issues
                    if (basePiece.ConstructionCompleted)
                    {
                        basePiece.ConstructionCompleted = false;
                    }
                    */
                }
                //This ensures that any pecice over 95% is completed and added to the completed base piece history
                if (basePiece.ConstructionAmount >= 0.95f && completedBasePieceHistory.Contains(basePiece) == false)
                {
                    basePiece.ConstructionAmount = 1.0f;
                    basePiece.ConstructionCompleted = true;
                    completedBasePieceHistory.Add(basePiece);
                }
            }
        }

        public void BasePieceConstructionCompleted(NitroxId id, NitroxId baseId)
        {
            BasePiece basePiece;

            lock (partiallyConstructedPiecesById)
            {
                if (partiallyConstructedPiecesById.TryGetValue(id, out basePiece))
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

                    //this task is now completed by BasePieceConstructionAmountChanged()
                    /*
                    lock (completedBasePieceHistory)
                    {
                        completedBasePieceHistory.Add(basePiece);
                    }
                    */
                }
            }
        }

        public void BasePieceDeconstructionBegin(NitroxId id)
        {
            BasePiece basePiece;

            lock (completedBasePieceHistory)
            {
                basePiece = completedBasePieceHistory.Find(piece => piece.Id == id);
                if (basePiece.ConstructionAmount < 1f && partiallyConstructedPiecesById.ContainsKey(basePiece.Id))
                {
                    completedBasePieceHistory.Remove(basePiece);
                    basePiece.ConstructionAmount = 0.95f;
                    basePiece.ConstructionCompleted = false;
                    // I am assuming this adds the base piece to the partially contructed list
                    lock (partiallyConstructedPiecesById)
                    {
                        partiallyConstructedPiecesById[basePiece.Id] = basePiece;
                    }
                }
                /*
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
                */
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
/*
                    if (basePiece.ConstructionAmount< 0.4f)
                    {
                        basePiece.ConstructionCompleted = false;
                        partiallyConstructedPiecesById.Remove(basePiece.Id);
                        completedBasePieceHistory.Remove(basePiece);
                    }
*/
