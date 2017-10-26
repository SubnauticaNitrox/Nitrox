using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionAmountChanged : PlayerActionPacket
    {
        public String Guid { get; }
        public float ConstructionAmount { get; }

        public ConstructionAmountChanged(String playerId, Vector3 itemPosition, String guid, float constructionAmount) : base(playerId, itemPosition)
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
