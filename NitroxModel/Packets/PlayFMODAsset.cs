using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayFMODAsset : Packet
    {
        public string AssetPath { get; }
        public NitroxVector3 Position { get; }
        public float Volume { get; set; }
        public float Radius { get; set; }
        public bool IsGlobal { get; }

        public PlayFMODAsset(string assetPath, NitroxVector3 position, float volume, float radius, bool isGlobal)
        {
            AssetPath = assetPath;
            Position = position;
            Volume = volume;
            Radius = radius;
            IsGlobal = isGlobal;
        }

        public override string ToString()
        {
            return $"[PlayFMODAsset - AssetPath: {AssetPath}, Position: {Position}, Volume {Volume}, Radius {Radius}, IsGlobal {IsGlobal}]";
        }
    }
}
