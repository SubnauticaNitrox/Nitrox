using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VisibleChunksChanged : AuthenticatedPacket
    {
        public List<SerializableInt3> Added { get; }
        public List<SerializableInt3> Removed { get; }

        public VisibleChunksChanged(String playerId, HashSet<Int3> added, HashSet<Int3> removed) : base(playerId)
        {
            //convert to list as hashsets have issues serializing in mono
            List<SerializableInt3> addedChunks = new List<SerializableInt3>();
            List<SerializableInt3> removedChunks = new List<SerializableInt3>();

            foreach(Int3 addedChunk in added)
            {
                addedChunks.Add(SerializableInt3.from(addedChunk));
            }

            foreach (Int3 removedChunk in removed)
            {
                removedChunks.Add(SerializableInt3.from(removedChunk));
            }

            this.Added = addedChunks;
            this.Removed = removedChunks;
        }

        public override string ToString()
        {
            String toString = "[ChunkLoaded Chunks: Added: | ";

            foreach(SerializableInt3 chunk in Added)
            {
                toString += chunk + " ";
            }

            toString += " | Removed: |";


            foreach (SerializableInt3 chunk in Removed)
            {
                toString += chunk + " ";
            }

            return toString + "| ]";
        }
    }
}
