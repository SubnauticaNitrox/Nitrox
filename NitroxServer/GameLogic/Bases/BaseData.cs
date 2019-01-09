using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;

namespace NitroxServer.GameLogic.Bases
{
    [ProtoContract]
    public class BaseData
    {
        [ProtoMember(1)]
        public Dictionary<string, BasePiece> SerializableBasePiecesByGuid
        {
            get
            {
                lock (basePiecesByGuid)
                {
                    return new Dictionary<string, BasePiece>(basePiecesByGuid);
                }
            }
            set { basePiecesByGuid = value; }
        }

        [ProtoMember(2)]
        public List<BasePiece> SerializableCompletedBasePieceHistory
        {
            get
            {
                lock (completedBasePieceHistory)
                {
                    return new List<BasePiece>(completedBasePieceHistory);
                }
            }
            set { completedBasePieceHistory = value; }
        }

        [ProtoIgnore]
        private Dictionary<string, BasePiece> basePiecesByGuid = new Dictionary<string, BasePiece>();

        [ProtoIgnore]
        private List<BasePiece> completedBasePieceHistory = new List<BasePiece>();

        public void AddBasePiece(BasePiece basePiece)
        {
            lock(basePiecesByGuid)
            {
                basePiecesByGuid.Add(basePiece.Guid, basePiece);
            }
        }

        public void BasePieceConstructionAmountChanged(string guid, float constructionAmount)
        {
            BasePiece basePiece;

            lock (basePiecesByGuid)
            {
                if (basePiecesByGuid.TryGetValue(guid, out basePiece))
                {
                    basePiece.ConstructionAmount = constructionAmount;

                    if(basePiece.ConstructionCompleted)
                    {
                        basePiece.ConstructionCompleted = false;
                    }
                }
            }
        }

        public void BasePieceConstructionCompleted(string guid, Optional<string> newlyCreatedParentGuid)
        {
            BasePiece basePiece;

            lock (basePiecesByGuid)
            {
                if (basePiecesByGuid.TryGetValue(guid, out basePiece))
                {
                    basePiece.ConstructionAmount = 1.0f;
                    basePiece.ConstructionCompleted = true;
                    basePiece.NewBaseGuid = newlyCreatedParentGuid;

                    if (newlyCreatedParentGuid.IsPresent())
                    {
                        basePiece.ParentGuid = Optional<string>.Empty();
                    }

                    lock(completedBasePieceHistory)
                    {
                        completedBasePieceHistory.Add(basePiece);
                    }
                }
            }
        }

        public void BasePieceDeconstructionBegin(string guid)
        {
            BasePiece basePiece;

            lock (basePiecesByGuid)
            {
                if (basePiecesByGuid.TryGetValue(guid, out basePiece))
                {
                    basePiece.ConstructionAmount = 0.95f;
                    basePiece.ConstructionCompleted = false;
                }
            }
        }

        public void BasePieceDeconstructionCompleted(string guid)
        {
            BasePiece basePiece;
            lock (basePiecesByGuid)
            {
                if (basePiecesByGuid.TryGetValue(guid, out basePiece))
                {
                    lock(completedBasePieceHistory)
                    {
                        completedBasePieceHistory.Remove(basePiece);
                    }

                    basePiecesByGuid.Remove(guid);
                }
            }
        }

        public void UpdateBasePieceMetadata(string guid, BasePieceMetadata metadata)
        {
            BasePiece basePiece;
            lock (basePiecesByGuid)
            {
                if (basePiecesByGuid.TryGetValue(guid, out basePiece))
                {
                    basePiece.Metadata = Optional<BasePieceMetadata>.OfNullable(metadata);
                }
            }
        }

        public List<BasePiece> GetBasePiecesForNewlyConnectedPlayer()
        {
            List<BasePiece> basePieces;
            
            lock(completedBasePieceHistory)
            {
                // Play back all completed base pieces first (other pieces have a dependency on these being done)
                basePieces = new List<BasePiece>(completedBasePieceHistory);
            }

            lock(basePiecesByGuid)
            {
                // Play back pieces that may not be completed yet.
                foreach (BasePiece basePiece in basePiecesByGuid.Values)
                {
                    if (!basePieces.Contains(basePiece))
                    {
                        basePieces.Add(basePiece);
                    }
                }
            }

            return basePieces;
        }
    }
}
