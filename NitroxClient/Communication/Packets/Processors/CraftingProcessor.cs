using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CrafterBeginCraftingProcessor : ClientPacketProcessor<CrafterBeginCrafting>
    {
        private readonly IPacketSender packetSender;

        public CrafterBeginCraftingProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CrafterBeginCrafting packet)
        {
            NitroxServiceLocator.LocateService<Crafting>().GhostCrafter_Remote_CraftingBegin(packet.CrafterGuid, packet.TechType, packet.Duration);
        }
    }

    public class CrafterItemPickupProcessor : ClientPacketProcessor<CrafterItemPickup>
    {
        private readonly IPacketSender packetSender;

        public CrafterItemPickupProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CrafterItemPickup packet)
        {
            NitroxServiceLocator.LocateService<Crafting>().CrafterLogic_Remote_ItemPickup(packet.CrafterGuid);
        }
    }

    public class CrafterEndCraftingProcessor : ClientPacketProcessor<CrafterEndCrafting>
    {
        private readonly IPacketSender packetSender;

        public CrafterEndCraftingProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CrafterEndCrafting packet)
        {
            NitroxServiceLocator.LocateService<Crafting>().GhostCrafter_Remote_CraftingEnd(packet.CrafterGuid);
        }
    }
}
