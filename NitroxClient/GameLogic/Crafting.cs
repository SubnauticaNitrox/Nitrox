using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Crafting
    {
        private readonly IPacketSender packetSender;

        public Crafting(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void FabricatorCrafingStarted(GameObject crafter, TechType techType, float duration)
        {
            string crafterGuid = GuidHelper.GetGuid(crafter);
            FabricatorBeginCrafting fabricatorBeginCrafting = new FabricatorBeginCrafting(crafterGuid, techType, duration);
            packetSender.Send(fabricatorBeginCrafting);
        }

        public void FabricatorItemPickedUp(GameObject gameObject, TechType techType)
        {
            string crafterGuid = GuidHelper.GetGuid(gameObject);

            FabricatorItemPickup fabricatorItemPickup = new FabricatorItemPickup(crafterGuid, techType);
            packetSender.Send(fabricatorItemPickup);
            Log.Debug(fabricatorItemPickup);
        }
    }
}
