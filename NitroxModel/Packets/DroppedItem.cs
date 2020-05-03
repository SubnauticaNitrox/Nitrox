using System;
using NitroxModel.DataStructures.Util;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DroppedItem : Packet
    {
        public DroppedItem(DTO.NitroxId id, Optional<DTO.NitroxId> waterParkId, DTO.TechType techType, DTO.Vector3 itemPosition, DTO.Quaternion itemRotation, byte[] bytes)
        {
            Id = id;
            WaterParkId = waterParkId;
            ItemPosition = itemPosition;
            ItemRotation = itemRotation;
            TechType = techType;
            Bytes = bytes;
        }

        public DTO.NitroxId Id { get; }
        public Optional<DTO.NitroxId> WaterParkId { get; }
        public DTO.TechType TechType { get; }
        public DTO.Vector3 ItemPosition { get; }
        public DTO.Quaternion ItemRotation { get; }
        public byte[] Bytes { get; }

        public override string ToString()
        {
            return "[DroppedItem - id: " + Id + " WaterParkId: " + WaterParkId + " techType: " + TechType + " itemPosition: " + ItemPosition + "]";
        }
    }
}
