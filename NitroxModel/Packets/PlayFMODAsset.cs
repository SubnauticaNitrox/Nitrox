using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayFMODAsset : Packet
    {
        public string AssetPath { get; }
        public NitroxVector3 Position { get; }
        public float Radius { get; }
        public bool IsGlobal { get; }
        public float Volume { get; set; } // Will be set by server (see PlayFMODAssetProcessor)

        public PlayFMODAsset(string assetPath, NitroxVector3 position, float radius, bool isGlobal)
        {
            AssetPath = assetPath;
            Position = position;
            Radius = radius;
            IsGlobal = isGlobal;
        }

        public override string ToString()
        {
            return $"[PlayFMODAsset - AssetPath: {AssetPath}, Position: {Position}, Radius {Radius}, Volume {Volume}, IsGlobal {IsGlobal}]";
        }
    }
}
