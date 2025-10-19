using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

[Serializable]
public class FMODStudioEmitterPacket : Packet
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
