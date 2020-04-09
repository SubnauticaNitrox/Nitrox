using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
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
            NitroxId crafterId = NitroxEntity.GetId(crafter);
            FabricatorBeginCrafting fabricatorBeginCrafting = new FabricatorBeginCrafting(crafterId, techType.Model(), duration);
            packetSender.Send(fabricatorBeginCrafting);
        }

        public void FabricatorItemPickedUp(GameObject gameObject, TechType techType)
        {
            NitroxId crafterId = NitroxEntity.GetId(gameObject);

            FabricatorItemPickup fabricatorItemPickup = new FabricatorItemPickup(crafterId, techType.Model());
            packetSender.Send(fabricatorItemPickup);
            Log.Debug($"{fabricatorItemPickup}");
        }
    }
}
