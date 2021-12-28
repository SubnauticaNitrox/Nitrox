using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class SubRootChanged : Packet
    {
        [Index(0)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(1)]
        public virtual Optional<NitroxId> SubRootId { get; protected set; }

        public SubRootChanged() { }

        public SubRootChanged(ushort playerId, Optional<NitroxId> subRootId)
        {
            PlayerId = playerId;
            SubRootId = subRootId;
        }
    }
}
