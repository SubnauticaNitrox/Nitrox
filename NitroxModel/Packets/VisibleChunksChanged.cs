using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VisibleChunksChanged : AuthenticatedPacket
    {
        public IEnumerable<Int3> Added { get { return convertAddedChunks(); } }
        public IEnumerable<Int3> Removed { get { return convertRemovedChunks(); } }

        private List<SerializableInt3> serializableAdded;
        private List<SerializableInt3> serializableRemoved;

        public VisibleChunksChanged(String playerId, HashSet<Int3> added, HashSet<Int3> removed) : base(playerId)
        {
            //convert to list as hashsets have issues serializing in mono
            List<SerializableInt3> addedChunks = new List<SerializableInt3>();
            List<SerializableInt3> removedChunks = new List<SerializableInt3>();

            foreach (Int3 addedChunk in added)
            {
                addedChunks.Add(SerializableInt3.from(addedChunk));
            }

            foreach (Int3 removedChunk in removed)
            {
                removedChunks.Add(SerializableInt3.from(removedChunk));
            }

            this.serializableAdded = addedChunks;
            this.serializableRemoved = removedChunks;
        }

        public override string ToString()
        {
            String toString = "[ChunkLoaded Chunks: Added: | ";

            foreach (SerializableInt3 chunk in serializableAdded)
            {
                toString += chunk + " ";
            }

            toString += " | Removed: |";


            foreach (SerializableInt3 chunk in serializableRemoved)
            {
                toString += chunk + " ";
            }

            return toString + "| ]";
        }

        private IEnumerable<Int3> convertAddedChunks()
        {
            foreach (SerializableInt3 addedChunk in serializableAdded)
            {
                yield return addedChunk.toInt3();
            }
        }

        private IEnumerable<Int3> convertRemovedChunks()
        {
            foreach (SerializableInt3 removedChunk in serializableRemoved)
            {
                yield return removedChunk.toInt3();
            }
        }
    }
}
