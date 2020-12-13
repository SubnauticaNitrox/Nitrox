using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
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
        public override string ToString()
        {
            return $"[WeldAction - Id: {Id}, HealthAdded: {HealthAdded}]";
        }
    }
}
