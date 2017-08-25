using NitroxModel.DataStructures;
using System.Collections.Generic;

namespace NitroxServer.Communication.Packets.Processors.WorldSending
{
    public class Chunk
    {
        public Int3 chunkCoord;
        public byte[] chunkData;

        public Chunk(Int3 chunkCoord, byte[] chunkData)
        {
            this.chunkCoord = chunkCoord;
            this.chunkData = chunkData;
        }
    }
}
