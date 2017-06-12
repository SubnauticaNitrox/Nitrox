using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PickupItem : PlayerActionPacket
    {
        public Vector3 ItemPosition { get; set; }
        public String GameObjectName { get; private set; }
        public String TechType { get; private set; }

        public PickupItem(String playerId, Vector3 playerPosition, Vector3 itemPosition, String gameObjectName, String techType) : base(playerId, playerPosition)
        {
            this.ItemPosition = itemPosition;
            this.GameObjectName = gameObjectName;
            this.TechType = techType;
        }

        public override string ToString()
        {
            return "[Pickup Item - ItemPosition: " + ItemPosition + " GameObjectName: " + GameObjectName + " TechType: " + TechType + "]";
        }
    }
}
