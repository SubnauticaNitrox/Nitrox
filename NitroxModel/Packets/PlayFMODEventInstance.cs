using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets;

[Serializable]
public class PlayFMODEventInstance : PlayFMODAsset
{
    public NitroxId Id { get; }
    public bool Play { get; }

    public PlayFMODEventInstance(NitroxId id, bool play, string assetPath, NitroxVector3 position, float volume, float radius, bool isGlobal) : base(assetPath, position, volume, radius, isGlobal)
    {
        Id = id;
        Play = play;
    }
}
