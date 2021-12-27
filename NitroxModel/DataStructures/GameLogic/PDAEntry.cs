using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class PDAEntry
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual NitroxTechType TechType { get; set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual float Progress { get; set; }

        [Index(2)]
        [ProtoMember(3)]
        public virtual int Unlocked { get; set; }

        protected PDAEntry()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public PDAEntry(NitroxTechType techType, float progress, int unlocked)
        {
            TechType = techType;
            Progress = progress;
            Unlocked = unlocked;
        }
    }
}
