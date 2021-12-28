using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class GhostCrafterBeginCrafting : Packet
    {
        [Index(0)]
        public virtual NitroxId GhostCrafterId { get; protected set; }
        [Index(1)]
        public virtual NitroxTechType TechType { get; protected set; }
        [Index(2)]
        public virtual float Duration { get; protected set; }

        public GhostCrafterBeginCrafting() { }

        public GhostCrafterBeginCrafting(NitroxId ghostCrafterId, NitroxTechType techType, float duration)
        {
            GhostCrafterId = ghostCrafterId;
            TechType = techType;
            Duration = duration;
        }
    }
}
