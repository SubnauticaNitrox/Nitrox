using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BaseConstructionCompleted : Packet
    {
        public NitroxId PieceId { get; }
        public NitroxId BaseId { get; }

        public BaseConstructionCompleted(NitroxId id, NitroxId baseId)
        {
            PieceId = id;
            BaseId = baseId;
        }

        public override string ToString()
        {
            return "[BaseConstructionCompleted Id: " + PieceId + " BaseId: " + BaseId + "]";
        }
    }
}
