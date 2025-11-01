using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class SeaTreaderChunkPickedUp : Packet
{
    public NitroxId ChunkId { get; }

    public SeaTreaderChunkPickedUp(NitroxId chunkId)
    {
        ChunkId = chunkId;
    }
}
