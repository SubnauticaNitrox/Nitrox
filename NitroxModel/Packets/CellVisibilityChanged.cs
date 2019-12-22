using System;
using System.Text;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CellVisibilityChanged : Packet
    {
        public NitroxId PlayerId { get; }
        public AbsoluteEntityCell[] Added { get; }
        public AbsoluteEntityCell[] Removed { get; }

        public CellVisibilityChanged(NitroxId playerId, AbsoluteEntityCell[] added, AbsoluteEntityCell[] removed)
        {
            PlayerId = playerId;
            Added = added;
            Removed = removed;
        }

        public override string ToString()
        {
            StringBuilder toString = new StringBuilder("[CellVisibilityChanged | Added: ");

            foreach (AbsoluteEntityCell visibleCell in Added)
            {
                toString.Append(visibleCell);
                toString.Append(' ');
            }

            toString.Append("| Removed: ");

            foreach (AbsoluteEntityCell visibleCell in Removed)
            {
                toString.Append(visibleCell);
                toString.Append(' ');
            }

            toString.Append("| PlayerId: ");
            toString.Append(PlayerId);
            toString.Append("]");

            return toString.ToString();
        }
    }
}
