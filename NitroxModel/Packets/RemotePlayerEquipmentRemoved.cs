using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RemotePlayerEquipmentRemoved : Packet
    {
        public ushort PlayerId { get; }
        public NitroxTechType TechType { get; }

        public RemotePlayerEquipmentRemoved(ushort playerId, NitroxTechType techType)
        {
            PlayerId = playerId;
            TechType = techType;
        }
    }
}
