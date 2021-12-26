using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class CellVisibilityChanged : Packet
    {
        [Index(0)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(1)]
        public virtual AbsoluteEntityCell[] Added { get; protected set; }
        [Index(2)]
        public virtual AbsoluteEntityCell[] Removed { get; protected set; }

        private CellVisibilityChanged() { }

        public CellVisibilityChanged(ushort playerId, AbsoluteEntityCell[] added, AbsoluteEntityCell[] removed)
        {
            PlayerId = playerId;
            Added = added;
            Removed = removed;
        }
    }
}
