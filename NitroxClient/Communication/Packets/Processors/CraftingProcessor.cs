using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CrafterStartUseProcessor : ClientPacketProcessor<CrafterStartUse>
    {
        private readonly IPacketSender packetSender;

        public CrafterStartUseProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CrafterStartUse packet)
        {
            GameLogic.Crafting.Instance.GhostCrafter_Remote_OnStartUse(packet.CrafterGuid, packet.PlayerName);
        }
    }

    public class CrafterEndUseProcessor : ClientPacketProcessor<CrafterEndUse>
    {
        private readonly IPacketSender packetSender;

        public CrafterEndUseProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CrafterEndUse packet)
        {
            GameLogic.Crafting.Instance.GhostCrafter_Remote_OnEndUse(packet.CrafterGuid);
        }
    }

    public class FabricatorBeginCraftingProcessor : ClientPacketProcessor<FabricatorBeginCrafting>
    {
        private readonly IPacketSender packetSender;

        public FabricatorBeginCraftingProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(FabricatorBeginCrafting packet)
        {
            GameLogic.Crafting.Instance.Fabricator_Remote_OnCraftingBegin(packet.FabricatorGuid, packet.TechType, packet.Duration);
        }
    }

    public class FabricatorEndCraftingProcessor : ClientPacketProcessor<FabricatorEndCrafting>
    {
        private readonly IPacketSender packetSender;

        public FabricatorEndCraftingProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(FabricatorEndCrafting packet)
        {
            GameLogic.Crafting.Instance.Fabricator_Remote_OnCraftingEnd(packet.FabricatorGuid);
        }
    }

    public class FabricatorItemPickupProcessor : ClientPacketProcessor<FabricatorItemPickup>
    {
        private readonly IPacketSender packetSender;

        public FabricatorItemPickupProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(FabricatorItemPickup packet)
        {
            GameLogic.Crafting.Instance.Fabricator_Remote_ItemPickup(packet.FabricatorGuid, packet.TechType);           
        }
    }
}
