using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VisibleChunksChanged : AuthenticatedPacket
    {
        public HashSet<Int3> Added { get; private set; }
        public HashSet<Int3> Removed { get; private set; }

        public VisibleChunksChanged(String playerId, HashSet<Int3> added, HashSet<Int3> removed) : base(playerId)
        {
            this.Added = added;
            this.Removed = removed;
        }

        public override string ToString()
        {
            String toString = "[ChunkLoaded Chunks: Added: | ";

            foreach(Int3 chunk in Added)
            {
                toString += chunk + " ";
            }

            toString += " | Removed: |";


            foreach (Int3 chunk in Removed)
            {
                toString += chunk + " ";
            }

            return toString + "| ]";
        }
    }
}
