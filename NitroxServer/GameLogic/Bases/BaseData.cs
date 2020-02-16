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
        private Dictionary<NitroxId, BasePiece> SerializableBasePiecesById
        {
            get
            {
                lock (basePiecesById)
                {
                    serializableBasePiecesById = new Dictionary<NitroxId, BasePiece>(basePiecesById);
                    return serializableBasePiecesById;
                }
            }
            set { serializableBasePiecesById = basePiecesById = value; }
        }

        [ProtoMember(2)]
        private Dictionary<NitroxId, BasePiece> serializableBasePiecesById = new Dictionary<NitroxId, BasePiece>();
        List<BasePiece> SerializableCompletedBasePieceHistory
        {
            get
            {
                lock (completedBasePieceHistory)
                {
                    serializableCompletedBasePieceHistory = new List<BasePiece>(completedBasePieceHistory);
                    return serializableCompletedBasePieceHistory;
                }
            }
            set { serializableCompletedBasePieceHistory = completedBasePieceHistory = value; }
        }

        private List<BasePiece> serializableCompletedBasePieceHistory = new List<BasePiece>();

        private Dictionary<NitroxId, BasePiece> basePiecesById = new Dictionary<NitroxId, BasePiece>();

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

        [ProtoAfterSerialization]
        private void ProtoAfterSerialization()
        {
            lock (completedBasePieceHistory)
            {
                completedBasePieceHistory = serializableCompletedBasePieceHistory;
            }

            lock (basePiecesById)
            {
                basePiecesById = serializableBasePiecesById;
            }
        }
    }
}
