using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RemotePlayerEquipmentAdded : Packet
    {
        public ushort PlayerId { get; }
        public NitroxTechType TechType { get; }

        public RemotePlayerEquipmentAdded(ushort playerId, NitroxTechType techType)
        {
            PlayerId = playerId;
            TechType = techType;
        }
    }
}
