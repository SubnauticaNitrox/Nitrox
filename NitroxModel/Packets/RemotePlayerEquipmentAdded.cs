using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RemotePlayerEquipmentAdded : Packet
    {
        public NitroxId PlayerId { get; }
        public TechType TechType { get; }

        public RemotePlayerEquipmentAdded(NitroxId playerId, TechType techType)
        {
            PlayerId = playerId;
            TechType = techType;
        }
    }
}
