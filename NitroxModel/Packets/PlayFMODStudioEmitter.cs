using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayFMODStudioEmitter : Packet
    {
        public NitroxId Id { get; }
        public string AssetPath { get; }
        public bool Play { get; }
        public bool AllowFadeout { get; }

        public PlayFMODStudioEmitter(NitroxId id, string assetPath, bool play, bool allowFadeout)
        {
            Id = id;
            AssetPath = assetPath;
            Play = play;
            AllowFadeout = allowFadeout;
        }
    }
}
