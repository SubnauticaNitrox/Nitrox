using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
{
    [Serializable]
    public class CyclopsFireSuppression : Packet
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

