using NitroxModel.DataStructures;
using System.Collections.Generic;

namespace NitroxServer.GameLogic
{
    public class ChunkManager
    {
        private Dictionary<Chunk, int> chunksByPlayerCount = new Dictionary<Chunk, int>();
        private HashSet<Chunk> newChunks = new HashSet<Chunk>();

        public void PlayerEnteredChunk(Chunk chunk)
        {
            lock (chunksByPlayerCount)
            {
                if (!chunksByPlayerCount.ContainsKey(chunk))
                {
                    chunksByPlayerCount[chunk] = 0;
                    newChunks.Add(chunk);
                }

                chunksByPlayerCount[chunk]++;
            }
        }

        public void PlayerLeftChunk(Chunk chunk)
        {
            lock (chunksByPlayerCount)
            {
                if (chunksByPlayerCount.ContainsKey(chunk))
                {
                    chunksByPlayerCount[chunk]--;
                }
            }
        }

        public HashSet<Chunk> GetAddedChunks()
        {
            HashSet<Chunk> addedChunks = new HashSet<Chunk>();

            lock (chunksByPlayerCount)
            {
                foreach(Chunk chunk in newChunks)
                {
                    if (chunksByPlayerCount.ContainsKey(chunk) && chunksByPlayerCount[chunk] > 0)
                    {
                        addedChunks.Add(chunk);
                    }
                }

                newChunks.Clear();
            }

            return addedChunks;
        }

        public HashSet<Chunk> GetRemovedChunks()
        {
            HashSet<Chunk> removedChunks = new HashSet<Chunk>();

            lock (chunksByPlayerCount)
            {
                foreach (Chunk chunk in chunksByPlayerCount.Keys)
                {
                    if (chunksByPlayerCount[chunk] <= 0)
                    {
                        removedChunks.Add(chunk);
                    }
                }

                foreach(Chunk chunk in removedChunks)
                {
                    chunksByPlayerCount.Remove(chunk);
                }
            }

            return removedChunks;
        }
    }
}
