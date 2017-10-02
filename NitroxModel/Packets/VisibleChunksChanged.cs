using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VisibleChunksChanged : AuthenticatedPacket
    {
        public List<Chunk> Added { get; }
        public List<Chunk> Removed { get; }
       
        public VisibleChunksChanged(String playerId, List<Chunk> added, List<Chunk> removed) : base(playerId)
        {
            this.Added = added;
            this.Removed = removed;
        }

        public override string ToString()
        {
            String toString = "[ChunkLoaded Chunks: Added: | ";

            foreach(Chunk chunk in Added)
            {
                toString += chunk + " ";
            }

            toString += " | Removed: |";


            foreach (Chunk chunk in Removed)
            {
                toString += chunk + " ";
            }

            return toString + "| ]";
        }
    }
}
