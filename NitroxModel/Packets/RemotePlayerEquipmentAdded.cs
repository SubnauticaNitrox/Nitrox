using System;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RemotePlayerEquipmentAdded : Packet
    {
        public ushort PlayerId { get; }
        public DTO.TechType TechType { get; }

        public RemotePlayerEquipmentAdded(ushort playerId, DTO.TechType techType)
        {
            PlayerId = playerId;
            TechType = techType;
        }
    }
}
