using NitroxModel.DataStructures.Util;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SubRootChanged : Packet
    {
        public ulong LPlayerId { get; }
        public Optional<string> SubRootGuid { get; }

        public SubRootChanged(ulong playerId, Optional<string> subRootGuid)
        {
            LPlayerId = playerId;
            SubRootGuid = subRootGuid;
        }
    }
}
