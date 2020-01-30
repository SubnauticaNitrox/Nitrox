using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures;

namespace NitroxServer.GameLogic.Bases
{
    [ProtoContract]
    public class BaseData
    {
        public const long VERSION = 1;
        [ProtoMember(1)]
        public Dictionary<NitroxId, BasePiece> SerializableBasePiecesById
        {
            get
            {
                lock (basePiecesById)
                {
                    return new Dictionary<NitroxId, BasePiece>(basePiecesById);
                }
            }
            set { basePiecesById = value; }
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
        private Dictionary<NitroxId, BasePiece> basePiecesById = new Dictionary<NitroxId, BasePiece>();

        [ProtoIgnore]
        private List<BasePiece> completedBasePieceHistory = new List<BasePiece>();

        public void AddBasePiece(BasePiece basePiece)
        {
            lock(basePiecesById)
            {
                basePiecesById.Add(basePiece.Id, basePiece);
            }
        }

        public void BasePieceConstructionAmountChanged(NitroxId id, float constructionAmount)
        {
            BasePiece basePiece;

            lock (basePiecesById)
            {
                if (basePiecesById.TryGetValue(id, out basePiece))
                {
                    basePiece.ConstructionAmount = constructionAmount;

                    if(basePiece.ConstructionCompleted)
                    {
                        basePiece.ConstructionCompleted = false;
                    }
                }
            }
        }

        public void BasePieceConstructionCompleted(NitroxId id, NitroxId baseId)
        {
            BasePiece basePiece;

            lock (basePiecesById)
            {
                if (basePiecesById.TryGetValue(id, out basePiece))
                {
                    basePiece.ConstructionAmount = 1.0f;
                    basePiece.ConstructionCompleted = true;
                    basePiece.BaseId = baseId;
                    basePiece.ParentId = Optional<NitroxId>.OfNullable(baseId);

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

            lock (basePiecesById)
            {
                if (basePiecesById.TryGetValue(id, out basePiece))
                {
                    basePiece.ConstructionAmount = 0.95f;
                    basePiece.ConstructionCompleted = false;
                }
            }
        }

        public void BasePieceDeconstructionCompleted(NitroxId id)
        {
            BasePiece basePiece;
            lock (basePiecesById)
            {
                if (basePiecesById.TryGetValue(id, out basePiece))
                {
                    lock(completedBasePieceHistory)
                    {
                        completedBasePieceHistory.Remove(basePiece);
                    }

                    basePiecesById.Remove(id);
                }
            }
        }

        public void UpdateBasePieceMetadata(NitroxId id, BasePieceMetadata metadata)
        {
            BasePiece basePiece;
            lock (basePiecesById)
            {
                if (basePiecesById.TryGetValue(id, out basePiece))
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

            lock(basePiecesById)
            {
                // Play back pieces that may not be completed yet.
                foreach (BasePiece basePiece in basePiecesById.Values)
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
