using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class ConstructionCompleted : Packet
    {
        [Index(0)]
        public virtual NitroxId PieceId { get; protected set; }
        [Index(1)]
        public virtual NitroxId BaseId { get; protected set; }

        public ConstructionCompleted() { }

        public ConstructionCompleted(NitroxId id, NitroxId baseId)
        {
            PieceId = id;
            BaseId = baseId;
        }
    }
}
