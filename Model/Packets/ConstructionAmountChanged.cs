using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructionAmountChanged : PlayerActionPacket
    { 
        public float ConstructionAmount { get; private set; }
        public Vector3 ItemPosition { get; private set; }

        public ConstructionAmountChanged(String playerId, Vector3 playerPosition, Vector3 itemPosition, float constructionAmount) : base(playerId, playerPosition)
        {
            this.ItemPosition = itemPosition;
            this.ConstructionAmount = constructionAmount;
        }
    }
}
