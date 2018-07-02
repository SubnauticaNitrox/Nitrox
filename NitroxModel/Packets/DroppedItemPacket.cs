using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DroppedItemPacket : Packet
    {
        public string Guid { get; }
        public Optional<string> WaterParkGuid { get; }
        public TechType TechType { get; }
        public Vector3 ItemPosition { get; }
        public byte[] Bytes { get; }

        public DroppedItemPacket(string guid, Optional<string> waterParkGuid, TechType techType, Vector3 itemPosition, byte[] bytes)
        {
            Guid = guid;
            WaterParkGuid = waterParkGuid;
            ItemPosition = itemPosition;
            TechType = techType;
            Bytes = bytes;
        }

        public override string ToString()
        {
            return "[DroppedItem Packet - guid: " + Guid + " WaterParkGuid: " + WaterParkGuid + " techType: " + TechType + " itemPosition: " + ItemPosition + "]";
        }
    }
}
