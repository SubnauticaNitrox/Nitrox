using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayFMODCustomLoopingEmitter : Packet
    {
        public NitroxId Id { get; }
        public string AssetPath { get; }

        public PlayFMODCustomLoopingEmitter(NitroxId id, string assetPath)
        {
            Id = id;
            AssetPath = assetPath;
        }
    }
}
