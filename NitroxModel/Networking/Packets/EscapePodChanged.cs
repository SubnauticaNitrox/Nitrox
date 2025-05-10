using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record EscapePodChanged : Packet
    {
        public ushort PlayerId { get; }
        public Optional<NitroxId> EscapePodId { get; }

        public EscapePodChanged(ushort playerId, Optional<NitroxId> escapePodId)
        {
            PlayerId = playerId;
            EscapePodId = escapePodId;
        }
    }
}
