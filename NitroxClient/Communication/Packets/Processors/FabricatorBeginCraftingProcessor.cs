using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using System;
using UnityEngine;
using System.Reflection;
using NitroxModel.Helper.GameLogic;

namespace NitroxClient.Communication.Packets.Processors
{
    public class FabricatorBeginCraftingProcessor : ClientPacketProcessor<FabricatorBeginCrafting>
    {
        private PacketSender packetSender;

        public FabricatorBeginCraftingProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(FabricatorBeginCrafting packet)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.FabricatorGuid);
            Fabricator fabricator = gameObject.GetComponentInChildren<Fabricator>(true);

            if (fabricator == null)
            {
                Console.WriteLine("Game object did not have a Fabricator component!");
                return;
            }
            
            float buildDuration = packet.Duration + 0.2f; // small increase to prevent this player from swiping item from remote player

            FieldInfo logic = typeof(Crafter).GetField("_logic", BindingFlags.Instance | BindingFlags.NonPublic);
            CrafterLogic crafterLogic = (CrafterLogic)logic.GetValue(fabricator);
            
            crafterLogic.Craft(packet.TechType, buildDuration);
        }
    }
}
