using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures;
using System.Linq;
using NitroxModel.DataStructures.Util;


namespace NitroxServer.GameLogic.Bases
{
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
            //just in case somehow a base piece is spawned at max completion
            //the base piece will get added to at least one directory
            if (basePiece.ConstructionAmount < 1.0f)
            {
                lock (partiallyConstructedPiecesById)
                {
                    partiallyConstructedPiecesById.Add(basePiece.Id, basePiece);
                }
                lock(completedBasePieceHistory)
                {
                    completedBasePieceHistory.Remove(basePiece);
                }
            }
            else
            {
                lock (completedBasePieceHistory)
                {
                    completedBasePieceHistory.Add(basePiece);
                }
                lock (partiallyConstructedPiecesById)
                {
                    partiallyConstructedPiecesById.Remove(basePiece.Id);
                }
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
                    if (basePiece.ConstructionCompleted)
                    {
                        basePiece.ConstructionCompleted = false;
                        
                        if (basePiece.ConstructionAmount != 1.0f && completedBasePieceHistory.Contains(basePiece) && !partiallyConstructedPiecesById.ContainsKey(basePiece.Id))
                        {
                            completedBasePieceHistory.Remove(basePiece);
                            if (!partiallyConstructedPiecesById.ContainsKey(basePiece.Id))
                            {
                                partiallyConstructedPiecesById.Add(basePiece.Id, basePiece);
                            }
                        }
                        
                    }
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
                    if (basePiece != null)
                    {
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
                    if (!partiallyConstructedPiecesById.ContainsKey(basePiece.Id))
                    {
                        partiallyConstructedPiecesById.Add(basePiece.Id, basePiece);
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

            lock (partiallyConstructedPiecesById)
            {
                // because base peice saving is weird I am going to comlete construction for all base peices first that did not save as completed
                // even if they were not completed before but only if they are above a certain %%
                // sometimes even base pieces were completed but dont get saved as completed so any piece over 40% will get added
                // at 100%   "BaseFoundation", "BaseRoom", "BaseMoonpool", "BaseObservatory", "BaseMapRoom", "BaseWindow", "BaseHatch", "BaseReinforcement"
                lock (completedBasePieceHistory)
                {
                    // Play back all completed base pieces first (other pieces have a dependency on these being done)
                    basePieces = new List<BasePiece>(completedBasePieceHistory);
                }
                
                //builds completed base foundation first
                foreach (BasePiece basePiece in completedBasePieceHistory)
                {
                    if (basePiece.TechType.Name == "BaseFoundation")
                    {
                        basePiece.ConstructionAmount = 1.0f;
                        basePiece.ConstructionCompleted = true;
                    }
                }
                //builds the foundations second
                foreach (BasePiece partialBasePiece in partiallyConstructedPiecesById.Values)
                {
                    if (partialBasePiece.TechType.Name == "BaseFoundation" && partialBasePiece.ConstructionAmount > 0.4f)
                    {
                        partialBasePiece.ConstructionAmount = 1.0f;
                        partialBasePiece.ConstructionCompleted = true;
                        basePieces.Add(partialBasePiece);
                    }
                }
                //builds the completed base rooms third
                foreach (BasePiece basePiece in completedBasePieceHistory)
                {
                    if (basePiece.TechType.Name == "BaseRoom" | basePiece.TechType.Name == "BaseMoonpool" | basePiece.TechType.Name == "BaseObservatory" | basePiece.TechType.Name == "BaseMapRoom")
                    {
                        basePiece.ConstructionAmount = 1.0f;
                        basePiece.ConstructionCompleted = true;
                    }
                }
                //builds the base rooms fourth
                foreach (BasePiece partialBasePiece in partiallyConstructedPiecesById.Values)
                {
                    if ((partialBasePiece.TechType.Name == "BaseRoom" | partialBasePiece.TechType.Name == "BaseMoonpool" | partialBasePiece.TechType.Name == "BaseObservatory" | partialBasePiece.TechType.Name == "BaseMapRoom") && partialBasePiece.ConstructionAmount > 0.4f)
                    {
                        partialBasePiece.ConstructionAmount = 1.0f;
                        partialBasePiece.ConstructionCompleted = true;
                        basePieces.Add(partialBasePiece);
                    }
                }
                //builds completed hatches/reinforcements/windows third
                foreach (BasePiece basePiece in completedBasePieceHistory)
                {
                    if (basePiece.TechType.Name == "BaseWindow" | basePiece.TechType.Name == "BaseHatch" | basePiece.TechType.Name == "BaseReinforcement")
                    {
                        basePiece.ConstructionAmount = 1.0f;
                        basePiece.ConstructionCompleted = true;
                    }
                }
                //builds uncompleted hatches/reinforcements/windows third
                foreach (BasePiece partialBasePiece in partiallyConstructedPiecesById.Values)
                {
                    if ((partialBasePiece.TechType.Name == "BaseWindow" | partialBasePiece.TechType.Name == "BaseHatch" | partialBasePiece.TechType.Name == "BaseReinforcement") && partialBasePiece.ConstructionAmount > 0.4f)
                    {
                        partialBasePiece.ConstructionAmount = 1.0f;
                        partialBasePiece.ConstructionCompleted = true;
                        basePieces.Add(partialBasePiece);
                    }
                }

                foreach (BasePiece partialBasePiece in partiallyConstructedPiecesById.Values)
                {
                    //security measure to make sure nothing is loaded at less than 100% 
                    if (partialBasePiece.ConstructionAmount > 0.4f && partialBasePiece.ConstructionAmount != 1.0f)
                    {
                        partialBasePiece.ConstructionAmount = 1.0f;
                        partialBasePiece.ConstructionCompleted = true;
                    }
                    if (!basePieces.Contains(partialBasePiece))
                    {
                        if (partialBasePiece.TechType.Name != "BaseWindow" && partialBasePiece.TechType.Name != "BaseHatch" && partialBasePiece.TechType.Name != "BaseReinforcement" && partialBasePiece.TechType.Name != "BaseRoom" && partialBasePiece.TechType.Name != "BaseMoonpool" && partialBasePiece.TechType.Name != "BaseObservatory" && partialBasePiece.TechType.Name != "BaseMapRoom" && partialBasePiece.TechType.Name != "BaseFoundation")
                        {
                            basePieces.Add(partialBasePiece);
                        }
                    }
                    if (partialBasePiece.ConstructionAmount < 0.4f && !completedBasePieceHistory.Contains(partialBasePiece))
                    {
                        partialBasePiece.ConstructionAmount = 0f;
                        partialBasePiece.ConstructionCompleted = false;
                    }
                }
                //clears partially constructed peieces bc there should be no uncompleted peices left because they have been destroyed or fully built
                //partiallyConstructedPiecesById.Clear();
            }
            return basePieces;
        }
    }
}


/*
                        lock (completedBasePieceHistory)
                        {
                            completedBasePieceHistory.Add(partialBasePiece);
                        }
                        lock (partiallyConstructedPiecesById)
                        {
                            partiallyConstructedPiecesById.Remove(partialBasePiece.Id);
                        }
*/
