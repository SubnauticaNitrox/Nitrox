using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class GhostCrafterItemPickup : Packet
    {
        [Index(0)]
        public virtual NitroxId GhostCrafterId { get; protected set; }
        [Index(1)]
        public virtual NitroxTechType TechType { get; protected set; }

        private GhostCrafterItemPickup() { }

        public GhostCrafterItemPickup(NitroxId ghostCrafterId, NitroxTechType techType)
        {
            GhostCrafterId = ghostCrafterId;
            TechType = techType;
        }
    }
}
