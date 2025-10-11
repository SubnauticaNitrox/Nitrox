using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

[Serializable]
public class SeaTreaderChunkPickedUp : Packet
{
    public NitroxId ChunkId { get; }

    public SeaTreaderChunkPickedUp(NitroxId chunkId)
    {
        ChunkId = chunkId;
    }
}
