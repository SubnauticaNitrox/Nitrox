using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record FMODCustomLoopingEmitterPacket : Packet
{
    public NitroxId Id { get; }
    public string AssetPath { get; }

    public FMODCustomLoopingEmitterPacket(NitroxId id, string assetPath)
    {
        Id = id;
        AssetPath = assetPath;
    }
}
