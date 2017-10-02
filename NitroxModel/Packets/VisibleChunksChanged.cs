using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VisibleChunksChanged : AuthenticatedPacket
    {
        public Chunk[] Added { get; }
        public Chunk[] Removed { get; }

        public VisibleChunksChanged(String playerId, Chunk[] added, Chunk[] removed) : base(playerId)
        {
            Added = added;
            Removed = removed;
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
