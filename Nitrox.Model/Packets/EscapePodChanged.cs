using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
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

