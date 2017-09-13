using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxServer.GameLogic.Monobehaviours
{
    public class ChunkLoader : MonoBehaviour
    {
        public static bool ALLOW_MAP_CLIPPING = false;

        public ChunkManager chunkManager { get; set; }

        private void Update()
        {
            ALLOW_MAP_CLIPPING = true;
            LoadChunks(chunkManager.GetChunksToLoad());
            UnloadChunks(chunkManager.GetChunksToUnload());
            ALLOW_MAP_CLIPPING = false;
        }

        private void LoadChunks(HashSet<Int3> chunks)
        {
            foreach(Int3 chunk in chunks)
            {
                Int3 blocksPerBatch = LargeWorldStreamer.main.blocksPerBatch;

                Int3 startBlocks = chunk * blocksPerBatch;
                Int3 endBlocks = chunk * blocksPerBatch;

                Int3.Bounds int3b = new Int3.Bounds(startBlocks, endBlocks);

                Dictionary<Int3, BatchCells> batch2cells = (Dictionary<Int3, BatchCells>)LargeWorldStreamer.main.cellManager.ReflectionGet("batch2cells");

                if (!batch2cells.ContainsKey(chunk))
                {
                    LargeWorldStreamer.main.ReflectionCall("TryUnloadBatch", false, false, chunk);
                    LargeWorldStreamer.main.LoadBatch(chunk);

                    //BatchCells batchCells = new BatchCells(LargeWorldStreamer.main.cellManager, LargeWorldStreamer.main, batch);
                    //batch2cells.Add(batch, batchCells);

                    LargeWorldStreamer.main.cellManager.ShowEntities(int3b, 0);
                    LargeWorldStreamer.main.cellManager.ShowEntities(int3b, 1);
                    LargeWorldStreamer.main.cellManager.ShowEntities(int3b, 2);
                    LargeWorldStreamer.main.cellManager.ShowEntities(int3b, 3);
                }

                Console.WriteLine("Loading chunk: " + chunk);
            }
        }

        private void UnloadChunks(HashSet<Int3> chunks)
        {
            foreach (Int3 chunk in chunks)
            {
                Int3 blocksPerBatch = LargeWorldStreamer.main.blocksPerBatch;

                Int3 end = chunk;

                Int3 startBlocks = chunk * blocksPerBatch;
                Int3 endBlocks = end * blocksPerBatch;

                Int3.Bounds int3b = new Int3.Bounds(startBlocks, endBlocks);

                LargeWorldStreamer.main.ReflectionCall("TryUnloadBatch", false, false, chunk);
                /*
                LargeWorldStreamer.main.cellManager.HideEntities(int3b, 0);
                LargeWorldStreamer.main.cellManager.HideEntities(int3b, 1);
                LargeWorldStreamer.main.cellManager.HideEntities(int3b, 2);
                LargeWorldStreamer.main.cellManager.HideEntities(int3b, 3);*/
                Console.WriteLine("Unloaded chunk: " + chunk);
            }
        }
    }
}
