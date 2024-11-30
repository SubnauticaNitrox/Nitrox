using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class SeaTreaderChunkPickedUp : Packet
{
    public NitroxId ChunkId { get; }

    public SeaTreaderChunkPickedUp(NitroxId chunkId)
    {
        ChunkId = chunkId;
    }
}
