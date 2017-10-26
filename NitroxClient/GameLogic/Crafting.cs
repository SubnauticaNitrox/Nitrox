using System;
using NitroxClient.Communication;
using NitroxModel.Packets;
using UnityEngine;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;

namespace NitroxClient.GameLogic
{
    public class Crafting
    {
        private readonly PacketSender packetSender;

        public Crafting(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void FabricatorCrafingStarted(GameObject crafter, TechType techType, float duration)
        {
            String crafterGuid = GuidHelper.GetGuid(crafter);
            FabricatorBeginCrafting fabricatorBeginCrafting = new FabricatorBeginCrafting(packetSender.PlayerId, crafterGuid, techType, duration);
            packetSender.Send(fabricatorBeginCrafting);
        }

        public void FabricatorItemPickedUp(GameObject gameObject, TechType techType)
        {
            String crafterGuid = GuidHelper.GetGuid(gameObject);

            FabricatorItemPickup fabricatorItemPickup = new FabricatorItemPickup(packetSender.PlayerId, crafterGuid, techType);
            packetSender.Send(fabricatorItemPickup);
            Log.Debug(fabricatorItemPickup);
        }
    }
}
