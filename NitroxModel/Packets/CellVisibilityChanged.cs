using System;
using System.Text;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CellVisibilityChanged : Packet
    {
        public ushort PlayerId { get; }
        public NitroxInt3[] Added { get; }
        public NitroxInt3[] Removed { get; }

        public CellVisibilityChanged(ushort playerId, NitroxInt3[] added, NitroxInt3[] removed)
        {
            PlayerId = playerId;
            Added = added;
            Removed = removed;
        }

        public override string ToString()
        {
            StringBuilder toString = new StringBuilder("[CellVisibilityChanged | Added: ");

            foreach (NitroxInt3 visibleCell in Added)
            {
                toString.Append(visibleCell);
                toString.Append(' ');
            }

            toString.Append("| Removed: ");

            foreach (NitroxInt3 visibleCell in Removed)
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
