using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RemotePlayerEquipmentRemoved : Packet
    {
        public ushort PlayerId { get; }
        public TechType TechType { get; }

        public RemotePlayerEquipmentRemoved(ushort playerId, TechType techType)
        {
            PlayerId = playerId;
            TechType = techType;
        }
    }
}
