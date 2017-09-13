using System.Collections.Generic;

namespace NitroxServer.GameLogic
{
    public class ChunkManager
    {
        private Dictionary<Int3, int> chunksByPlayerCount = new Dictionary<Int3, int>();
        private HashSet<Int3> newChunks = new HashSet<Int3>();

        public void PlayerEnteredChunk(Int3 chunk)
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

        public void PlayerLeftChunk(Int3 chunk)
        {
            lock (chunksByPlayerCount)
            {
                if (chunksByPlayerCount.ContainsKey(chunk))
                {
                    chunksByPlayerCount[chunk]--;
                }
            }
        }

        public HashSet<Int3> GetChunksToLoad()
        {
            HashSet<Int3> chunksToLoad = new HashSet<Int3>();

            lock (chunksByPlayerCount)
            {
                foreach(Int3 chunk in newChunks)
                {
                    if (chunksByPlayerCount.ContainsKey(chunk) && chunksByPlayerCount[chunk] > 0)
                    {
                        chunksToLoad.Add(chunk);
                    }
                }

                newChunks.Clear();
            }

            return chunksToLoad;
        }

        public HashSet<Int3> GetChunksToUnload()
        {
            HashSet<Int3> chunksToUnload = new HashSet<Int3>();

            lock (chunksByPlayerCount)
            {
                foreach (Int3 chunk in chunksByPlayerCount.Keys)
                {
                    if (chunksByPlayerCount[chunk] <= 0)
                    {
                        chunksToUnload.Add(chunk);
                    }
                }

                foreach(Int3 chunk in chunksToUnload)
                {
                    chunksByPlayerCount.Remove(chunk);
                }
            }

            return chunksToUnload;
        }
    }
}
