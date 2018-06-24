using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionAmountChanged : Packet
    {
        public string Guid { get; }
        public float ConstructionAmount { get; }

        public ConstructionAmountChanged(string guid, float constructionAmount)
        {
            Guid = guid;
            ConstructionAmount = constructionAmount;
        }

        public override string ToString()
        {
            return "[ConstructionAmountChanged Guid:" + Guid + " ConstructionAmount: " + ConstructionAmount + "]";
        }
    }
}
