using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VisibleChunksChanged : AuthenticatedPacket
    {
        public Int3[] Added { get; }
        public Int3[] Removed { get; }

        public VisibleChunksChanged(String playerId, Int3[] added, Int3[] removed) : base(playerId)
        {
            Added = added;
            Removed = removed;
        }

        public override string ToString()
        {
            String toString = "[ChunkLoaded Chunks: Added: | ";

            foreach (Int3 chunk in Added)
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
