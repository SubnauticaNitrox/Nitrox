using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionAmountChanged : Packet
    {
        public NitroxId Id { get; }
        public float ConstructionAmount { get; }

        public ConstructionAmountChanged(NitroxId id, float constructionAmount)
        {
            Id = id;
            ConstructionAmount = constructionAmount;
        }

        public override string ToString()
        {
            return $"[ConstructionAmountChanged - Id: {Id}, ConstructionAmount: {ConstructionAmount}]";
        }
    }
}
