using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DroppedItem : PlayerActionPacket
    {
        public String Guid { get; }
        public Optional<String> WaterParkGuid { get; }
        public TechType TechType { get { return serializableTechType.TechType; } }
        public Vector3 ItemPosition { get { return serializableItemPosition.ToVector3(); } }
        public byte[] Bytes { get; }
        
        private SerializableVector3 serializableItemPosition;
        private SerializableTechType serializableTechType;

        public DroppedItem(String playerId, String guid, Optional<String> waterParkGuid, TechType techType, Vector3 itemPosition, byte[] bytes) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.WaterParkGuid = waterParkGuid;
            this.serializableItemPosition = SerializableVector3.from(itemPosition);
            this.serializableTechType = new SerializableTechType(techType);
            this.Bytes = bytes;
        }

        public override string ToString()
        {
            return "[DroppedItem - playerId: " + PlayerId + " guid: " + Guid + " WaterParkGuid: " + WaterParkGuid + " techType: " + TechType + " itemPosition: " + serializableItemPosition + "]";
        }
    }
}
