using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class FabricatorBeginCraftingProcessor : ClientPacketProcessor<FabricatorBeginCrafting>
    {
        private readonly IPacketSender packetSender;

        public FabricatorBeginCraftingProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(FabricatorBeginCrafting packet)
        {
            GameObject gameObject = NitroxIdentifier.RequireObjectFrom(packet.FabricatorId);
            Fabricator fabricator = gameObject.RequireComponentInChildren<Fabricator>(true);

            float buildDuration = packet.Duration + 0.2f; // small increase to prevent this player from swiping item from remote player

            FieldInfo logic = typeof(Crafter).GetField("_logic", BindingFlags.Instance | BindingFlags.NonPublic);
            CrafterLogic crafterLogic = (CrafterLogic)logic.GetValue(fabricator);

            crafterLogic.Craft(packet.TechType.Enum(), buildDuration);
        }
    }
}
