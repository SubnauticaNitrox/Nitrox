using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PDAEntryRemove : Packet
    {
        [Index(0)]
        public virtual NitroxTechType TechType { get; protected set; }

        public PDAEntryRemove() { }

        public PDAEntryRemove(NitroxTechType techType)
        {
            TechType = techType;
        }
    }
}
