using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayFMODCustomLoopingEmitter : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual string AssetPath { get; protected set; }

        private PlayFMODCustomLoopingEmitter() { }

        public PlayFMODCustomLoopingEmitter(NitroxId id, string assetPath)
        {
            Id = id;
            AssetPath = assetPath;
        }
    }
}
