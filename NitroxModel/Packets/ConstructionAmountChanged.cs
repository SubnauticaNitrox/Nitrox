using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class ConstructionAmountChanged : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual float ConstructionAmount { get; protected set; }

        public ConstructionAmountChanged() { }

        public ConstructionAmountChanged(NitroxId id, float constructionAmount)
        {
            Id = id;
            ConstructionAmount = constructionAmount;
        }
    }
}
