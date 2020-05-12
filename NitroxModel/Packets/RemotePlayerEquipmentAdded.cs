using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RemotePlayerEquipmentAdded : Packet
    {
        public ushort PlayerId { get; }
        public DataStructures.TechType TechType { get; }

        public RemotePlayerEquipmentAdded(ushort playerId, DataStructures.TechType techType)
        {
            PlayerId = playerId;
            TechType = techType;
        }
    }
}
