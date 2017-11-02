using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionAmountChanged : PlayerActionPacket
    {
        public string Guid { get; }
        public float ConstructionAmount { get; }

        public ConstructionAmountChanged(Vector3 itemPosition, string guid, float constructionAmount) : base(itemPosition)
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
