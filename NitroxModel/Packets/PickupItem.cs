using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PickupItem : PlayerActionPacket
    {
        public String Guid { get; }
        public Vector3 ItemPosition { get; }
        public String TechType { get; }

        public PickupItem(String playerId, Vector3 itemPosition, String guid, String techType) : base(playerId, itemPosition)
        {
            ItemPosition = itemPosition;
            Guid = guid;
            TechType = techType;
        }

        public override string ToString()
        {
            return "[Pickup Item - ItemPosition: " + ItemPosition + " Guid: " + Guid + " TechType: " + TechType + "]";
        }
    }
}
