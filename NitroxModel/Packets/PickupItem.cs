using NitroxModel.DataStructures;
using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PickupItem : PlayerActionPacket
    {
        public String Guid { get; }
        public Vector3 ItemPosition { get { return serializableItemPosition.ToVector3(); } }
        public String TechType { get; }
        
        private SerializableVector3 serializableItemPosition { get; }

        public PickupItem(String playerId, Vector3 itemPosition, String guid, String techType) : base(playerId, itemPosition)
        {
            this.serializableItemPosition = SerializableVector3.from(itemPosition);
            this.Guid = guid;
            this.TechType = techType;
        }

        public override string ToString()
        {
            return "[Pickup Item - ItemPosition: " + serializableItemPosition + " Guid: " + Guid + " TechType: " + TechType + "]";
        }
    }
}
