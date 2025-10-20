using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.Subnautica.Packets;

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
