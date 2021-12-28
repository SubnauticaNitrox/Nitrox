using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PDAEntryProgress : Packet
    {
        [Index(0)]
        public virtual NitroxTechType TechType { get; protected set; }
        [Index(1)]
        public virtual float Progress { get; protected set; }
        [Index(2)]
        public virtual int Unlocked { get; protected set; }
        [Index(3)]
        public virtual NitroxId NitroxId { get; protected set; }

        public PDAEntryProgress() { }

        public PDAEntryProgress(NitroxTechType techType, float progress, int unlocked, NitroxId nitroxId)
        {
            TechType = techType;
            Progress = progress;
            Unlocked = unlocked;
            NitroxId = nitroxId;
        }
    }
}
