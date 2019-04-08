using NitroxModel.DataStructures.Util;
using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SubRootChanged : Packet
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
