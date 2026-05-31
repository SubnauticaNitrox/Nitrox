using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
{
    [Serializable]
    public class EscapePodChanged : Packet
    {
        public SessionId SessionId { get; }
        public Optional<NitroxId> EscapePodId { get; }

        public EscapePodChanged(SessionId sessionId, Optional<NitroxId> escapePodId)
        {
            SessionId = sessionId;
            EscapePodId = escapePodId;
        }
    }
}
