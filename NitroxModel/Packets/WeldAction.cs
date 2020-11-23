using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
