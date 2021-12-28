using NitroxModel.DataStructures.Unity;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayFMODAsset : Packet
    {
        [Index(0)]
        public virtual string AssetPath { get; protected set; }
        [Index(1)]
        public virtual NitroxVector3 Position { get; protected set; }
        [Index(2)]
        public virtual float Volume { get; set; }
        [Index(3)]
        public virtual float Radius { get; protected set; }
        [Index(4)]
        public virtual bool IsGlobal { get; protected set; }

        public PlayFMODAsset() { }

        public PlayFMODAsset(string assetPath, NitroxVector3 position, float volume, float radius, bool isGlobal)
        {
            AssetPath = assetPath;
            Position = position;
            Volume = volume;
            Radius = radius;
            IsGlobal = isGlobal;
        }
    }
}
