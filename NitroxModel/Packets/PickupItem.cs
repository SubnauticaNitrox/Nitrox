using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PickupItem : PlayerActionPacket
    {
        public string Guid { get; }
        public Vector3 ItemPosition { get; }
        public string TechType { get; }

        public PickupItem(string playerId, Vector3 itemPosition, string guid, string techType) : base(playerId, itemPosition)
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
