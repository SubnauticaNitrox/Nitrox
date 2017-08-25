using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ChangeBatchCell : AuthenticatedPacket
    {
        public byte[] ChunkChanges { get; private set; }
        public Int3 ChunkLocation { get; private set; }

        public ChangeBatchCell(string playerId, byte[] chunkChanges, Int3 chunkLocation) : base(playerId)
        {
            this.ChunkChanges = chunkChanges;
            this.ChunkLocation = chunkLocation;
        }
    }
}