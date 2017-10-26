using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionAmountChanged : PlayerActionPacket
    {
        public string Guid { get; }
        public float ConstructionAmount { get; }

        public ConstructionAmountChanged(string playerId, Vector3 itemPosition, string guid, float constructionAmount) : base(playerId, itemPosition)
        {
            Guid = guid;
            ConstructionAmount = constructionAmount;
        }

        public override string ToString()
        {
            return "[ConstructionAmountChanged( - playerId: " + PlayerId + " Guid:" + Guid + " ConstructionAmount: " + ConstructionAmount + "]";
        }
    }
}
