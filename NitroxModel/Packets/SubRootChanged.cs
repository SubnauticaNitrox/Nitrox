using NitroxModel.DataStructures.Util;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SubRootChanged : Packet
    {
        public ulong PlayerId { get; }
        public Optional<string> SubRootGuid { get; }

        public SubRootChanged(ulong playerId, Optional<string> subRootGuid)
        {
            PlayerId = playerId;
            SubRootGuid = subRootGuid;
        }
    }
}
