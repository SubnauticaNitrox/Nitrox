using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class DroppedItem : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual Optional<NitroxId> WaterParkId { get; protected set; }
        [Index(2)]
        public virtual NitroxTechType TechType { get; protected set; }
        [Index(3)]
        public virtual NitroxVector3 ItemPosition { get; protected set; }
        [Index(4)]
        public virtual NitroxQuaternion ItemRotation { get; protected set; }
        [Index(5)]
        public virtual byte[] Bytes { get; protected set; }

        public DroppedItem() { }

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
