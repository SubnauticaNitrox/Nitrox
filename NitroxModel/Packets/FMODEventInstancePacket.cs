using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets;

[Serializable]
public class FMODEventInstancePacket : FMODAssetPacket
{
    public NitroxId Id { get; }
    public bool Play { get; }

    public FMODEventInstancePacket(NitroxId id, bool play, string assetPath, NitroxVector3 position, float volume) : base(assetPath, position, volume)
    {
        Id = id;
        Play = play;
    }
}
