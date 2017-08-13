using System;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Crafting
    {
        private PacketSender packetSender;

        public Crafting(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void FabricatorCrafingStarted(GameObject crafter, TechType techType, float duration)
        {
            String crafterGuid = GuidHelper.GetGuid(crafter);
            String apiTechType = ApiHelper.TechType(techType);
            FabricatorBeginCrafting fabricatorBeginCrafting = new FabricatorBeginCrafting(packetSender.PlayerId, crafterGuid, apiTechType, duration);
            packetSender.Send(fabricatorBeginCrafting);
        }

        public void FabricatorItemPickedUp(GameObject gameObject, TechType techType)
        {
            String crafterGuid = GuidHelper.GetGuid(gameObject);
            String apiTechType = ApiHelper.TechType(techType);

            FabricatorItemPickup fabricatorItemPickup = new FabricatorItemPickup(packetSender.PlayerId, crafterGuid, apiTechType);
            packetSender.Send(fabricatorItemPickup);
            Console.WriteLine(fabricatorItemPickup);
        }
    }
}
