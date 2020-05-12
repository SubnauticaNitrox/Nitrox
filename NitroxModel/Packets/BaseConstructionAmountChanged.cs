using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BaseConstructionAmountChanged : Packet
    {
        public NitroxId Id { get; }
        public float ConstructionAmount { get; }

        public BaseConstructionAmountChanged(NitroxId id, float constructionAmount)
        {
            Id = id;
            ConstructionAmount = constructionAmount;
        }

        public override string ToString()
        {
            return "[BaseConstructionAmountChanged Id:" + Id + " ConstructionAmount: " + ConstructionAmount + "]";
        }
    }
}
