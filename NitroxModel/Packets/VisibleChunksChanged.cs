using NitroxModel.DataStructures;
using System;
using System.Text;

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
            StringBuilder toString = new StringBuilder("[ChunkLoaded Chunks: Added: | ");

            foreach (Chunk chunk in Added)
            {
                toString.Append(chunk);
                toString.Append(' ');
            }

            toString.Append("| Removed: | ");

            foreach (Chunk chunk in Removed)
            {
                toString.Append(chunk);
                toString.Append(' ');
            }

            toString.Append("| ]");
            return toString.ToString();
        }
    }
}
