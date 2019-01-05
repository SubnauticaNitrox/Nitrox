using NitroxModel.DataStructures.Util;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SubRootChanged : Packet
    {
        public ushort PlayerId { get; }
        public Optional<string> SubRootGuid { get; }

        public SubRootChanged(ushort playerId, Optional<string> subRootGuid)
        {
            PlayerId = playerId;
            SubRootGuid = subRootGuid;
        }

        public override string ToString()
        {
            return "[SubRootChanged - PlayerId: " + PlayerId + " SubRootGuid: " + SubRootGuid + "]";
        }
    }
}
