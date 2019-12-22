using NitroxModel.DataStructures.Util;
using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SubRootChanged : Packet
    {
        public NitroxId PlayerId { get; }
        public Optional<NitroxId> SubRootId { get; }

        public SubRootChanged(NitroxId playerId, Optional<NitroxId> subRootId)
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
