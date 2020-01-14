using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DroppedItem : Packet
    {
        public NitroxId Id { get; }
        public Optional<NitroxId> WaterParkId { get; }
        public TechType TechType { get; }
        public Vector3 ItemPosition { get; }
        public Quaternion ItemRotation { get; }
        public byte[] Bytes { get; }

        public DroppedItem(NitroxId id, Optional<NitroxId> waterParkId, TechType techType, Vector3 itemPosition, Quaternion itemRotation, byte[] bytes)
        {
            Id = id;
            WaterParkId = waterParkId;
            ItemPosition = itemPosition;
            ItemRotation = itemRotation;
            TechType = techType;
            Bytes = bytes;
        }        

        public override string ToString()
        {
            return "[DroppedItem - id: " + Id + " WaterParkId: " + WaterParkId + " techType: " + TechType + " itemPosition: " + ItemPosition + "]";
        }
    }
}
