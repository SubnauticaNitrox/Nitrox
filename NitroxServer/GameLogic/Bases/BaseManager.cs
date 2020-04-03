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

            //playing with speed above 1 or fastbuild enabled can yeild 0% completion to 100% completion with no intermediate
            //not sure if necessary
            //UNSURE IF SAFE TO REVERT TO ORIGINAL
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
                        //ONLY ADDS TO PARTIALLY CONSTRUCTED IF ITS NOT ALREADY IN PARTIALLY CONSTRUCTED
                        //UNSURE IF SAFE TO REMOVE
                        if (basePiece.ConstructionAmount != 1.0f && completedBasePieceHistory.Contains(basePiece) && !partiallyConstructedPiecesById.ContainsKey(basePiece.Id))
                        {
                            completedBasePieceHistory.Remove(basePiece);
                            partiallyConstructedPiecesById.Add(basePiece.Id, basePiece);
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
                    //NOT SURE IF THIS CAUSES ISSUES OR IS UNNECESSARY
                    //UNSURE IF SAFE TO REMOVE
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
                    //MAYBE UNNECESSARY
                    //UNSURE IF SAFE TO REMOVE
                    if (!partiallyConstructedPiecesById.ContainsKey(basePiece.Id))
                    {
                        //MUST KEEP
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
                    // I removed adding the base pieces until after the base piece "reboot"
                    basePieces = new List<BasePiece>();
                }

                //NOT SURE IF ADDING COMPLETEDBASEPIECEHISTORY IS NECESSARY AT ALL 
                //JUST TRYING TO MAKE SURE NOTHING SLIDES
                //PROBABLY SAFE TO REMOVE
                
                //builds completed base foundation first
                /*
                foreach (BasePiece basePiece in completedBasePieceHistory)
                {
                    if (basePiece.TechType.Name == "BaseFoundation")
                    {
                        basePiece.ConstructionAmount = 1.0f;
                        basePiece.ConstructionCompleted = true;
                    }
                }
                */
                //completes the uncompleted foundations
                foreach (BasePiece partialBasePiece in partiallyConstructedPiecesById.Values)
                {
                    if (partialBasePiece.TechType.Name == "BaseFoundation" && partialBasePiece.ConstructionAmount > 0.4f)
                    {
                        // when these pieces are built I would like to remove them from partiallyConstructedPiecesById and add them to completedBasePieceHistory
                        // but threading issues pulls up an error, it seems to work without this but it could also create some other problems
                        partialBasePiece.ConstructionAmount = 1.0f;
                        partialBasePiece.ConstructionCompleted = true;
                        // not sure if this is necessary but it seems to help
                        // this adds the partial piece to the list just like the other partial pieces
                        basePieces.Add(partialBasePiece);
                    }
                }
                //PROBABLY SAFE TO REMOVE
                //builds the completed base rooms third
                /*
                foreach (BasePiece basePiece in completedBasePieceHistory)
                {
                    if (basePiece.TechType.Name == "BaseRoom" | basePiece.TechType.Name == "BaseMoonpool" | basePiece.TechType.Name == "BaseObservatory" | basePiece.TechType.Name == "BaseMapRoom")
                    {
                        basePiece.ConstructionAmount = 1.0f;
                        basePiece.ConstructionCompleted = true;
                    }
                }
                */
                //completes the uncompleted base rooms
                foreach (BasePiece partialBasePiece in partiallyConstructedPiecesById.Values)
                {
                    if ((partialBasePiece.TechType.Name == "BaseRoom" | partialBasePiece.TechType.Name == "BaseMoonpool" | partialBasePiece.TechType.Name == "BaseMapRoom") && partialBasePiece.ConstructionAmount > 0.4f)
                    {
                        partialBasePiece.ConstructionAmount = 1.0f;
                        partialBasePiece.ConstructionCompleted = true;
                        basePieces.Add(partialBasePiece);
                    }
                }
                //PROBABLY SAFE TO REMOVE
                //builds completed hatches/reinforcements/windows third
                /*
                foreach (BasePiece basePiece in completedBasePieceHistory)
                {
                    if (basePiece.TechType.Name == "BaseWindow" | basePiece.TechType.Name == "BaseHatch" | basePiece.TechType.Name == "BaseReinforcement")
                    {
                        basePiece.ConstructionAmount = 1.0f;
                        basePiece.ConstructionCompleted = true;
                    }
                }
                */
                //completes uncompleted hatches/reinforcements/windows third
                foreach (BasePiece partialBasePiece in partiallyConstructedPiecesById.Values)
                {
                    if ((partialBasePiece.TechType.Name == "BaseWindow" | partialBasePiece.TechType.Name == "BaseHatch" | partialBasePiece.TechType.Name == "BaseReinforcement") && partialBasePiece.ConstructionAmount > 0.4f)
                    {
                        partialBasePiece.ConstructionAmount = 1.0f;
                        partialBasePiece.ConstructionCompleted = true;
                        basePieces.Add(partialBasePiece);
                    }
                }
                // adds in completed base pieces after the other base peices have been fixed
                basePieces.AddRange(completedBasePieceHistory);

                foreach (BasePiece partialBasePiece in partiallyConstructedPiecesById.Values)
                {
                    //security measure to make sure nothing is loaded at less than 100% 
                    // this does mean anything over 40% will be loaded to 100%
                    if (partialBasePiece.ConstructionAmount > 0.4f && partialBasePiece.ConstructionAmount != 1.0f)
                    {
                        partialBasePiece.ConstructionAmount = 1.0f;
                        partialBasePiece.ConstructionCompleted = true;
                    }
                    //adds partial pieces as normal but prevents duplicates from the previous code
                    if (!basePieces.Contains(partialBasePiece))
                    {
                        if (partialBasePiece.TechType.Name != "BaseWindow" && partialBasePiece.TechType.Name != "BaseHatch" && partialBasePiece.TechType.Name != "BaseReinforcement" && partialBasePiece.TechType.Name != "BaseRoom" && partialBasePiece.TechType.Name != "BaseMoonpool" && partialBasePiece.TechType.Name != "BaseObservatory" && partialBasePiece.TechType.Name != "BaseMapRoom" && partialBasePiece.TechType.Name != "BaseFoundation")
                        {
                            basePieces.Add(partialBasePiece);
                        }
                    }
                    //removes any peice that is under 40% completion
                    //PROBABLY SAFE TO REMOVE
                    /*
                    if (partialBasePiece.ConstructionAmount < 0.4f && !completedBasePieceHistory.Contains(partialBasePiece))
                    {
                        partialBasePiece.ConstructionAmount = 0f;
                        partialBasePiece.ConstructionCompleted = false;
                    }
                    */
            }
        }
            return basePieces;
        }
    }
}
