using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
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
