using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionCompleted : Packet
    {
        public NitroxId PieceId { get; }
        public NitroxId BaseId { get; }

        public ConstructionCompleted(NitroxId pieceId, NitroxId baseId)
        {
            PieceId = pieceId;
            BaseId = baseId;
        }
    }
}
