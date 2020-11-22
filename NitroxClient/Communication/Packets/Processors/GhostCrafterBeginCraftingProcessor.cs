using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class GhostCrafterBeginCraftingProcessor : ClientPacketProcessor<GhostCrafterBeginCrafting>
    {
        private readonly IPacketSender packetSender;

        public GhostCrafterBeginCraftingProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(GhostCrafterBeginCrafting packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.GhostCrafterId);
            GhostCrafter ghostCrafter = gameObject.RequireComponentInChildren<GhostCrafter>(true);

            float buildDuration = packet.Duration + 0.2f; // small increase to prevent this player from swiping item from remote player

            FieldInfo logic = typeof(Crafter).GetField("_logic", BindingFlags.Instance | BindingFlags.NonPublic);
            CrafterLogic crafterLogic = (CrafterLogic)logic.GetValue(ghostCrafter);

            crafterLogic.Craft(packet.TechType.ToUnity(), buildDuration);
        }
    }
}
