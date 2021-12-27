using NitroxModel.DataStructures;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class CyclopsDestroyed : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }

        private CyclopsDestroyed() { }

        public CyclopsDestroyed(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"[CyclopsDestroyed - Id: {Id}]";
        }
    }
}
