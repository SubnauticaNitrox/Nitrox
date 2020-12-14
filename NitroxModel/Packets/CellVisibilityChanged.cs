using System;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CellVisibilityChanged : Packet
    {
        public ushort PlayerId { get; }
        public AbsoluteEntityCell[] Added { get; }
        public AbsoluteEntityCell[] Removed { get; }

        public CellVisibilityChanged(ushort playerId, AbsoluteEntityCell[] added, AbsoluteEntityCell[] removed)
        {
            PlayerId = playerId;
            Added = added;
            Removed = removed;
        }

        public override string ToString()
        {
            return $"[CellVisibilityChanged - PlayerID: {PlayerId}, Added: {Added?.Length}, Removed: {Removed?.Length}]";
        }

        public override string ToLongString()
        {
            return $"[CellVisibilityChanged - PlayerID: {PlayerId}, Added: ({string.Join(", ", Added.ToList())}), Removed: ({string.Join(", ", Removed.ToList())})]";
        }
    }
}
