using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record SubRootChanged : Packet
    {
        public ushort PlayerId { get; }
        public Optional<NitroxId> SubRootId { get; }

        public SubRootChanged(ushort playerId, Optional<NitroxId> subRootId)
        {
            PlayerId = playerId;
            SubRootId = subRootId;
        }
    }
}
