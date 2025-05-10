using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record FMODStudioEmitterPacket : Packet
{
    public NitroxId Id { get; }
    public string AssetPath { get; }
    public bool Play { get; }
    public bool AllowFadeout { get; }

    public FMODStudioEmitterPacket(NitroxId id, string assetPath, bool play, bool allowFadeout)
    {
        Id = id;
        AssetPath = assetPath;
        Play = play;
        AllowFadeout = allowFadeout;
    }
}
