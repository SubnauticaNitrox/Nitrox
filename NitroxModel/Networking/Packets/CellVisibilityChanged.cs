using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record CellVisibilityChanged : Packet
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
    }
}
