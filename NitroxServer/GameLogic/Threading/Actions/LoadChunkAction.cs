using System;
using NitroxModel.Helper;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Threading.Actions
{
    public class LoadChunkAction : IGameAction
    {
        private Int3 batch;

        public LoadChunkAction(Int3 batch)
        {
            this.batch = batch;
        }

        public void Execute()
        {
            NitroxServer.Server.ALLOW_MAP_CLIPPING = true;
            Int3 blocksPerBatch = LargeWorldStreamer.main.blocksPerBatch;
            
            Int3 startBlocks = batch * blocksPerBatch;
            Int3 endBlocks = batch * blocksPerBatch;

            Int3.Bounds int3b = new Int3.Bounds(startBlocks, endBlocks);
            
            Dictionary<Int3, BatchCells> batch2cells = (Dictionary<Int3, BatchCells>)LargeWorldStreamer.main.cellManager.ReflectionGet("batch2cells");

            if(!batch2cells.ContainsKey(batch))
            {
                LargeWorldStreamer.main.ReflectionCall("TryUnloadBatch", false, false, batch);
                LargeWorldStreamer.main.LoadBatch(batch);

                BatchCells batchCells = new BatchCells(LargeWorldStreamer.main.cellManager, LargeWorldStreamer.main, batch);
                batch2cells.Add(batch, batchCells);

                LargeWorldStreamer.main.cellManager.ShowEntities(int3b, 0);
                LargeWorldStreamer.main.cellManager.ShowEntities(int3b, 1);
                LargeWorldStreamer.main.cellManager.ShowEntities(int3b, 2);
                LargeWorldStreamer.main.cellManager.ShowEntities(int3b, 3);
            }

            Console.WriteLine("Loading chunk: " + batch);
            NitroxServer.Server.ALLOW_MAP_CLIPPING = false;
        }
    }
}
