using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
{
    [Serializable]
    public class WeldAction : Packet
    {
        public NitroxId Id { get; }
        public float HealthAdded { get; }

        public WeldAction(NitroxId id, float healthAdded)
        {
            Id = id;
            HealthAdded = healthAdded;
        }
    }
}
