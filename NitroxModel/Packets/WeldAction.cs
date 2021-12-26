using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class WeldAction : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual float HealthAdded { get; protected set; }

        private WeldAction() { }

        public WeldAction(NitroxId id, float healthAdded)
        {
            Id = id;
            HealthAdded = healthAdded;
        }
    }
}
