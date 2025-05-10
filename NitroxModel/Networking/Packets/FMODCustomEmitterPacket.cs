using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record FMODCustomEmitterPacket : Packet
{
    public NitroxId Id { get; }
    public string AssetPath { get; }
    public bool Play { get; }

    public FMODCustomEmitterPacket(NitroxId id, string assetPath, bool play)
    {
        Id = id;
        AssetPath = assetPath;
        Play = play;
    }
}
