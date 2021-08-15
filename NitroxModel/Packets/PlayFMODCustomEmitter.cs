using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayFMODCustomEmitter : Packet
    {
        public NitroxId Id { get; }
        public string AssetPath { get; }
        public bool Play { get; }

        public PlayFMODCustomEmitter(NitroxId id, string assetPath, bool play)
        {
            Id = id;
            AssetPath = assetPath;
            Play = play;
        }
    }
}
