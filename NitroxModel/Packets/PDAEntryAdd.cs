using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PDAEntryAdd : Packet
    {
        [Index(0)]
        public virtual NitroxTechType TechType { get; protected set; }
        [Index(1)]
        public virtual float Progress { get; protected set; }
        [Index(2)]
        public virtual int Unlocked { get; protected set; }

        public PDAEntryAdd() { }

        public PDAEntryAdd(NitroxTechType techType, float progress, int unlocked)
        {
            TechType = techType;
            Progress = progress;
            Unlocked = unlocked;
        }
    }
}
