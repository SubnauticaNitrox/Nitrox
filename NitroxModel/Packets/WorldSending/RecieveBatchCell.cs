using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RecieveBatchCell : AuthenticatedPacket
    {
        public byte[] ChunkChanges { get; private set; }
        public Int3 ChunkLocation { get; private set; }
        public bool StillMore { get; private set; }

        public RecieveBatchCell(string playerId, byte[] chunkChanges, Int3 chunkLocation, bool stillMore) : base(playerId)
        {
            this.ChunkChanges = chunkChanges;
            this.ChunkLocation = chunkLocation;
            this.StillMore = stillMore;
        }
    }
}
