using System;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RemotePlayerEquipmentRemoved : Packet
    {
        public ushort PlayerId { get; }
        public DTO.TechType TechType { get; }

        public RemotePlayerEquipmentRemoved(ushort playerId, DTO.TechType techType)
        {
            PlayerId = playerId;
            TechType = techType;
        }
    }
}
