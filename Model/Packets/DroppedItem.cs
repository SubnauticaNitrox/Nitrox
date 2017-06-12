using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DroppedItem : PlayerActionPacket
    {
        public String TechType { get; set; }
        public Vector3 PushVelocity { get; set; }
        public Vector3 ItemPosition { get; set; }

        public DroppedItem(String playerId, String techType, Vector3 playerPosition, Vector3 itemPosition, Vector3 pushVelocity) : base(playerId, playerPosition)
        {
            this.TechType = techType;
            this.PushVelocity = pushVelocity;
        }
    }
}
