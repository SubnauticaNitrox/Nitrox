using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class EscapePodChanged : Packet
    {
        [Index(0)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(1)]
        public virtual Optional<NitroxId> EscapePodId { get; protected set; }

        private EscapePodChanged() { }

        public EscapePodChanged(ushort playerId, Optional<NitroxId> escapePodId)
        {
            PlayerId = playerId;
            EscapePodId = escapePodId;
        }
    }
}

