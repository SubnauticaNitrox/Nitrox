using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DroppedItem : Packet
    {
        public NitroxId Id { get; }
        public Optional<NitroxId> WaterParkId { get; }
        public NitroxTechType TechType { get; }
        public NitroxVector3 ItemPosition { get; }
        public NitroxQuaternion ItemRotation { get; }
        public byte[] Bytes { get; }

        public DroppedItem(NitroxId id, Optional<NitroxId> waterParkId, NitroxTechType techType, NitroxVector3 itemPosition, NitroxQuaternion itemRotation, byte[] bytes)
        {
            Id = id;
            WaterParkId = waterParkId;
            ItemPosition = itemPosition;
            ItemRotation = itemRotation;
            TechType = techType;
            Bytes = bytes;
        }
    }
}
