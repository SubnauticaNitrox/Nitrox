using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RemotePlayerEquipmentRemoved : Packet
    {
        public NitroxId PlayerId { get; }
        public TechType TechType { get; }

        public RemotePlayerEquipmentRemoved(NitroxId playerId, TechType techType)
        {
            PlayerId = playerId;
            TechType = techType;
        }
    }
}
