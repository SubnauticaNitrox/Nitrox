using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BaseConstructionCompleted : Packet
    {
        public NitroxId PieceId { get; }

        public BaseConstructionCompleted(NitroxId id)
        {
            PieceId = id;
        }

        public override string ToString()
        {
            return "[BaseConstructionCompleted Id: " + PieceId + "]";
        }
    }
}
