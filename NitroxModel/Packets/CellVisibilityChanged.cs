using NitroxModel.DataStructures;
using System;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CellVisibilityChanged : AuthenticatedPacket
    {
        public VisibleCell[] Added { get; }
        public VisibleCell[] Removed { get; }

        public CellVisibilityChanged(String playerId, VisibleCell[] added, VisibleCell[] removed) : base(playerId)
        {
            Added = added;
            Removed = removed;
        }

        public override string ToString()
        {
            StringBuilder toString = new StringBuilder("[CellVisibilityChanged | Added: ");

            foreach (VisibleCell visibleCell in Added)
            {
                toString.Append(visibleCell);
                toString.Append(' ');
            }

            toString.Append("| Removed: ");

            foreach (VisibleCell visibleCell in Removed)
            {
                toString.Append(visibleCell);
                toString.Append(' ');
            }

            toString.Append("| ]");
            return toString.ToString();
        }
    }
}
