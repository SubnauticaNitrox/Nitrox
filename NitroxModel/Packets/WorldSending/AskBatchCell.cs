using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets.WorldSending
{
    [Serializable]
    public class AskBatchCell : AuthenticatedPacket
    {
        public bool FromChunkQueue { get; private set; }
        public Int3 ChunkLocation { get; private set; }

        public AskBatchCell(string playerId, bool fromChunkQueue) : this(playerId, fromChunkQueue, new Int3(0, 0, 0)) { }

        public AskBatchCell(string playerId, bool fromChunkQueue, Int3 chunkLocation) : base(playerId)
        {
            this.FromChunkQueue = fromChunkQueue;
            this.ChunkLocation = chunkLocation;
        }
    }
}
