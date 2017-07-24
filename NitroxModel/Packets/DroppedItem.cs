using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DroppedItem : PlayerActionPacket
    {
        public String Guid { get; private set; }
        public Optional<String> WaterParkGuid { get; private set; }
        public String TechType { get; private set; }
        public Vector3 ItemPosition { get; private set; }
        public byte[] Bytes { get; private set; }

        public DroppedItem(String playerId, String guid, Optional<String> waterParkGuid, String techType, Vector3 itemPosition, byte[] bytes) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.WaterParkGuid = waterParkGuid;
            this.ItemPosition = itemPosition;
            this.TechType = techType;
            this.Bytes = bytes;
        }

        public override string ToString()
        {
            return "[DroppedItem - playerId: " + PlayerId + " guid: " + Guid + " WaterParkGuid: " + WaterParkGuid + " techType: " + TechType + " itemPosition: " + ItemPosition + "]";
        }
    }
}
