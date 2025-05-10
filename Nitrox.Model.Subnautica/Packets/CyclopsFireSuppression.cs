using System;
using NitroxModel.DataStructures;
using NitroxModel.Networking.Packets;

namespace Nitrox.Model.Subnautica.Packets
{
    [Serializable]
    public record CyclopsFireSuppression : Packet
    {
        public NitroxId Id { get; }

        public CyclopsFireSuppression(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"[CyclopsFireSuppressionSystem - Id: {Id}]";
        }
    }
}
