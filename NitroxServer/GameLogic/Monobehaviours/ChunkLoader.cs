using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxServer.GameLogic.Monobehaviours
{
    public class ChunkLoader : MonoBehaviour
    {
        public static bool ALLOW_MAP_CLIPPING = false;

        public ChunkManager chunkManager { get; set; }
        
        private Dictionary<Int3, int> levelLoadedForBatch = new Dictionary<Int3, int>();
        
        private void Update()
        {
            ALLOW_MAP_CLIPPING = true;
            LoadChunks(chunkManager.GetAddedChunks());
            UnloadChunks(chunkManager.GetRemovedChunks());
            ALLOW_MAP_CLIPPING = false;
        }

        private void LoadChunks(HashSet<Chunk> addedChunks)
        {
            if (LargeWorldStreamer.main.clips != null)
            {
                LargeWorldStreamer.main.clips.debugDisableVisibilityPhase = true;
            }

            foreach (Chunk chunk in addedChunks)
            {
                bool chunkLoaded = levelLoadedForBatch.ContainsKey(chunk.BatchId);
                int currentLoadedLevelForChunk = 3;

                if (chunkLoaded)
                {
                    currentLoadedLevelForChunk = levelLoadedForBatch[chunk.BatchId];
                }
                else
                {
                    LargeWorldStreamer.main.LoadBatch(chunk.BatchId);
                    Console.WriteLine("loaded chunk: " + chunk);
                }

                Int3.Bounds int3b = LargeWorldStreamer.main.GetBatchBlockBounds(chunk.BatchId);
            
                for (int i = currentLoadedLevelForChunk; i >= chunk.Level; i--)
                {
                    LargeWorldStreamer.main.cellManager.ShowEntities(int3b, i);
                    levelLoadedForBatch[chunk.BatchId] = i;
                }
            }
        }
        
        private void UnloadChunks(HashSet<Chunk> chunks)
        {
            foreach (Chunk chunk in chunks)
            {
                Int3.Bounds int3b = LargeWorldStreamer.main.GetBatchBlockBounds(chunk.BatchId);
                
                LargeWorldStreamer.main.cellManager.HideEntities(int3b, chunk.Level);

                levelLoadedForBatch[chunk.BatchId] = (chunk.Level - 1);

                if (chunk.Level == 3)
                {
                    LargeWorldStreamer.main.UnloadBatch(chunk.BatchId);
                    levelLoadedForBatch.Remove(chunk.BatchId);
                    Console.WriteLine("Unloaded chunk: " + chunk);
                }                
            }
        }
    }
}
