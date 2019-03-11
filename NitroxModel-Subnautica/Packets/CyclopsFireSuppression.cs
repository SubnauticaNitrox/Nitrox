using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsFireSuppression : Packet
    {
        public string Guid { get; }

        public CyclopsFireSuppression(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsFireSuppressionSystem Guid: " + Guid + "]";
        }
    }
}

