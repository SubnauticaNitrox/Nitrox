using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;

namespace Nitrox.Client.GameLogic
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
            FabricatorBeginCrafting fabricatorBeginCrafting = new FabricatorBeginCrafting(crafterId, techType.ToDto(), duration);
            packetSender.Send(fabricatorBeginCrafting);
        }

        public void FabricatorItemPickedUp(GameObject gameObject, TechType techType)
        {
            NitroxId crafterId = NitroxEntity.GetId(gameObject);

            FabricatorItemPickup fabricatorItemPickup = new FabricatorItemPickup(crafterId, techType.ToDto());
            packetSender.Send(fabricatorItemPickup);
            Log.Debug(fabricatorItemPickup);
        }
    }
}
