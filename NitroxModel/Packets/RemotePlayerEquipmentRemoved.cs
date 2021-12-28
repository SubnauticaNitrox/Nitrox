using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class RemotePlayerEquipmentRemoved : Packet
    {
        [Index(0)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(1)]
        public virtual NitroxTechType TechType { get; protected set; }

        public RemotePlayerEquipmentRemoved() { }

        public RemotePlayerEquipmentRemoved(ushort playerId, NitroxTechType techType)
        {
            PlayerId = playerId;
            TechType = techType;
        }
    }
}
