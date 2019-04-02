using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DroppedItem : Packet
    {
        public string Guid { get; }
        public Optional<string> WaterParkGuid { get; }
        public TechType TechType { get; }
        public Vector3 ItemPosition { get; }
        public Quaternion ItemRotation { get; }
        public byte[] Bytes { get; }

        public DroppedItem(string guid, Optional<string> waterParkGuid, TechType techType, Vector3 itemPosition, Quaternion itemRotation, byte[] bytes)
        {
            Guid = guid;
            WaterParkGuid = waterParkGuid;
            ItemPosition = itemPosition;
            ItemRotation = itemRotation;
            TechType = techType;
            Bytes = bytes;
        }        

        public override string ToString()
        {
            return "[DroppedItem - guid: " + Guid + " WaterParkGuid: " + WaterParkGuid + " techType: " + TechType + " itemPosition: " + ItemPosition + "]";
        }
    }
}
