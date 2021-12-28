using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayFMODStudioEmitter : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual string AssetPath { get; protected set; }
        [Index(2)]
        public virtual bool Play { get; protected set; }
        [Index(3)]
        public virtual bool AllowFadeout { get; protected set; }

        public PlayFMODStudioEmitter() { }

        public PlayFMODStudioEmitter(NitroxId id, string assetPath, bool play, bool allowFadeout)
        {
            Id = id;
            AssetPath = assetPath;
            Play = play;
            AllowFadeout = allowFadeout;
        }
    }
}
