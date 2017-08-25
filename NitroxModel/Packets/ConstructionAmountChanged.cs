using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionAmountChanged : PlayerActionPacket
    {
        public String Guid { get; private set; }
        public float ConstructionAmount { get; private set; }
        
        public ConstructionAmountChanged(String playerId, Vector3 itemPosition, String guid, float constructionAmount) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.ConstructionAmount = constructionAmount;
            this.PlayerMustBeInRangeToReceive = false;
        }

        public override string ToString()
        {
            return "[ConstructionAmountChanged( - playerId: " + PlayerId + " Guid:" + Guid + " ConstructionAmount: " + ConstructionAmount + "]";
        }
    }
}
