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
        public String Guid { get; private set; }
        public String TechType { get; private set; }
        public Vector3 PushVelocity { get; private set; }
        public Vector3 ItemPosition { get; private set; }

        public DroppedItem(String playerId, String guid, String techType, Vector3 itemPosition, Vector3 pushVelocity) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.ItemPosition = itemPosition;
            this.TechType = techType;
            this.PushVelocity = pushVelocity;
        }
    }
}
