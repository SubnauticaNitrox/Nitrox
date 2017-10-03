using NitroxModel.DataStructures;
using System.Collections.Generic;

namespace NitroxClient.Map
{
    public class LoadedChunks
    {
        private HashSet<Chunk> chunks = new HashSet<Chunk>();

        public void Add(Chunk chunk)
        {
            lock (chunks)
            {
                chunks.Add(chunk);
            }
        }

        public void Remove(Chunk chunk)
        {
            lock (chunks)
            {
                chunks.Remove(chunk);
            }
        }

        public bool Contains(Chunk chunk)
        {
            lock (chunks)
            {
                return chunks.Contains(chunk);
            }
        }

        public bool HasChunkWithMinDesiredLevelOfDetail(Int3 batchId, int level)
        {
            lock (chunks)
            {
                foreach (Chunk chunk in chunks)
                {
                    if(chunk.BatchId == batchId && chunk.Level <= level)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
