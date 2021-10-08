using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionCompleted : Packet
    {
        public NitroxId PieceId { get; }
        public NitroxId BaseId { get; }

        public ConstructionCompleted(NitroxId id, NitroxId baseId)
        {
            PieceId = id;
            BaseId = baseId;
        }
    }
}
