using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets.Core;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RemotePlayerEquipmentRemoved : Packet, IVolatilePacket
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
