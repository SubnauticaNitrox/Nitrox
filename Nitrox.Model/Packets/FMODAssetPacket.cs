using System;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.Packets;

[Serializable]
public class FMODAssetPacket : Packet
{
    public string AssetPath { get; }
    public NitroxVector3 Position { get; }
    public float Volume { get; set; }

    public FMODAssetPacket(string assetPath, NitroxVector3 position, float volume)
    {
        AssetPath = assetPath;
        Position = position;
        Volume = volume;
    }
}
