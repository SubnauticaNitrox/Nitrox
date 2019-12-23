using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RemotePlayerEquipmentAdded : Packet
    {
        public ushort PlayerId { get; }
        public TechType TechType { get; }

        public RemotePlayerEquipmentAdded(ushort playerId, TechType techType)
        {
            PlayerId = playerId;
            TechType = techType;
        }
    }
}
