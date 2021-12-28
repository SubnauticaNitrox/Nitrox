using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class KnownTechEntryAdd : Packet
    {
        [Index(0)]
        public virtual NitroxTechType TechType { get; protected set; }
        [Index(1)]
        public virtual bool Verbose { get; protected set; }

        public KnownTechEntryAdd() { }

        public KnownTechEntryAdd(NitroxTechType techType, bool verbose)
        {
            TechType = techType;
            Verbose = verbose;
        }
    }
}
