using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RemotePlayerEquipmentRemoved : Packet
    {
        public ushort PlayerId { get; }
        public DataStructures.TechType TechType { get; }

        public RemotePlayerEquipmentRemoved(ushort playerId, DataStructures.TechType techType)
        {
            PlayerId = playerId;
            TechType = techType;
        }
    }
}
