using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayFMODAsset : Packet
    {
        public string Id { get; }
        public NitroxVector3 Position { get; }
        public float Radius { get; }
        public float Volume { get; set; } // Will be set by server (see PlayFMODAssetProcessor)

        public PlayFMODAsset(string id, NitroxVector3 position, float radius)
        {
            Id = id;
            Position = position;
            Radius = radius;
        }

        public override string ToString()
        {
            return $"[PlayFMODAsset - Id: {Id}, Position: {Position}, Radius {Radius}, Volume {Volume}]";
        }
    }
}
