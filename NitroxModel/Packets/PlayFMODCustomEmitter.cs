using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayFMODCustomEmitter : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual string AssetPath { get; protected set; }
        [Index(2)]
        public virtual bool Play { get; protected set; }

        public PlayFMODCustomEmitter() { }

        public PlayFMODCustomEmitter(NitroxId id, string assetPath, bool play)
        {
            Id = id;
            AssetPath = assetPath;
            Play = play;
        }
    }
}
