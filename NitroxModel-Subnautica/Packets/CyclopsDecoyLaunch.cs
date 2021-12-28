using NitroxModel.DataStructures;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class CyclopsDecoyLaunch : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }

        public CyclopsDecoyLaunch() { }

        public CyclopsDecoyLaunch(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"[CyclopsDecoyLaunch - Id: {Id}]";
        }
    }
}
