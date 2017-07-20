using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PickupItem : PlayerActionPacket
    {
        public String Guid { get; private set; }
        public Vector3 ItemPosition { get; private set; }
        public String TechType { get; private set; }

        public PickupItem(String playerId, Vector3 itemPosition, String guid, String techType) : base(playerId, itemPosition)
        {
            this.ItemPosition = itemPosition;
            this.Guid = guid;
            this.TechType = techType;
        }

        public override string ToString()
        {
            return "[Pickup Item - ItemPosition: " + ItemPosition + " Guid: " + Guid + " TechType: " + TechType + "]";
        }
    }
}
