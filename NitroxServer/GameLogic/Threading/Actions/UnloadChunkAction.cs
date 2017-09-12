using NitroxModel.Helper;
using System;

namespace NitroxServer.GameLogic.Threading.Actions
{
    public class UnloadChunkAction : IGameAction
    {
        private Int3 batch;

        public UnloadChunkAction(Int3 batch)
        {
            this.batch = batch;
        }

        public void Execute()
        {
            NitroxServer.Server.ALLOW_MAP_CLIPPING = true;
            Int3 blocksPerBatch = LargeWorldStreamer.main.blocksPerBatch;

            Int3 end = new Int3(batch.x, batch.y, batch.z);

            Int3 startBlocks = batch * blocksPerBatch;
            Int3 endBlocks = end * blocksPerBatch;

            Int3.Bounds int3b = new Int3.Bounds(startBlocks, endBlocks);

            LargeWorldStreamer.main.ReflectionCall("TryUnloadBatch", false, false, batch);
            LargeWorldStreamer.main.LoadBatch(batch);

            LargeWorldStreamer.main.cellManager.HideEntities(int3b, 0);
            LargeWorldStreamer.main.cellManager.HideEntities(int3b, 1);
            LargeWorldStreamer.main.cellManager.HideEntities(int3b, 2);
            LargeWorldStreamer.main.cellManager.HideEntities(int3b, 3);
            Console.WriteLine("Unloaded chunk: " + batch);
            NitroxServer.Server.ALLOW_MAP_CLIPPING = false;
        }
    }
}
