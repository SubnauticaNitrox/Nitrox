using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record SeaTreaderChunkPickedUp : Packet
{
    public NitroxId ChunkId { get; }

    public SeaTreaderChunkPickedUp(NitroxId chunkId)
    {
        ChunkId = chunkId;
    }
}
