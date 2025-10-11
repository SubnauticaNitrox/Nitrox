using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Util;

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
    }
}
