using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DroppedItem : RangedPacket
    {
        public string Guid { get; }
        public Optional<string> WaterParkGuid { get; }
        public TechType TechType { get; }
        public Vector3 ItemPosition { get; }
        public byte[] Bytes { get; }

        public DroppedItem(string guid, Optional<string> waterParkGuid, TechType techType, Vector3 itemPosition, byte[] bytes) : base(itemPosition)
        {
            Guid = guid;
            WaterParkGuid = waterParkGuid;
            ItemPosition = itemPosition;
            TechType = techType;
            Bytes = bytes;
        }

        public override string ToString()
        {
            return "[DroppedItem - guid: " + Guid + " WaterParkGuid: " + WaterParkGuid + " techType: " + TechType + " itemPosition: " + ItemPosition + "]";
        }
    }
}
