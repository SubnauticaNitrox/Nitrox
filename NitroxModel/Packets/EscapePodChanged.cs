using NitroxModel.DataStructures.Util;
using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EscapePodChanged : Packet
    {        
        public ushort PlayerId { get; }
        public Optional<NitroxId> EscapePodId { get; }

        public EscapePodChanged(ushort playerId, Optional<NitroxId> escapePodId)
        {
            PlayerId = playerId;
            EscapePodId = escapePodId;
        }

        public override string ToString()
        {
            return "[EscapePodChanged - PlayerId: " + PlayerId + " SubRootId: " + EscapePodId + "]";
        }
    }
}

