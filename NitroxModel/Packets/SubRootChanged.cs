using NitroxModel.DataStructures.Util;
using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets.Core;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SubRootChanged : Packet, IVolatilePacket
    {
        public ushort PlayerId { get; }
        public Optional<NitroxId> SubRootId { get; }

        public SubRootChanged(ushort playerId, Optional<NitroxId> subRootId)
        {
            PlayerId = playerId;
            SubRootId = subRootId;
        }

        public override string ToString()
        {
            return "[SubRootChanged - PlayerId: " + PlayerId + " SubRootId: " + SubRootId + "]";
        }
    }
}
