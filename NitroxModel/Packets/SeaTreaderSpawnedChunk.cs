using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets;

[Serializable]
public class SeaTreaderSpawnedChunk : Packet
{
    public NitroxId CreatureId { get; }
    public NitroxId ChunkId { get; }
    public NitroxVector3 Position { get; }
    public NitroxQuaternion Rotation { get; }

    public SeaTreaderSpawnedChunk(NitroxId creatureId, NitroxId chunkId, NitroxVector3 position, NitroxQuaternion rotation)
    {
        CreatureId = creatureId;
        ChunkId = chunkId;
        Position = position;
        Rotation = rotation;
    }
}
