using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;
using System.Reflection;

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
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(packet.FabricatorGuid);

            if (opGameObject.IsEmpty())
            {
                Console.WriteLine("Could not find fabricator from guid " + packet.FabricatorGuid);
                return;
            }

            GameObject gameObject = opGameObject.Get();
            Fabricator fabricator = gameObject.GetComponentInChildren<Fabricator>(true);

            if (fabricator == null)
            {
                Console.WriteLine("Game object did not have a Fabricator component!");
                return;
            }

            Optional<TechType> opTechType = ApiHelper.TechType(packet.TechType);

            if (opTechType.IsEmpty())
            {
                Console.WriteLine("Trying to build unknown tech type: " + packet.TechType + " - ignoring.");
                return;
            }

            TechType techType = opTechType.Get();
            float buildDuration = packet.Duration + 0.2f; // small increase to prevent this player from swiping item from remote player

            FieldInfo logic = typeof(Crafter).GetField("_logic", BindingFlags.Instance | BindingFlags.NonPublic);
            CrafterLogic crafterLogic = (CrafterLogic)logic.GetValue(fabricator);
            
            crafterLogic.Craft(techType, buildDuration);
        }
    }
}
