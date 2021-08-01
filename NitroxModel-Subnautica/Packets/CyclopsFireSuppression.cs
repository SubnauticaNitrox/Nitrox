using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
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

