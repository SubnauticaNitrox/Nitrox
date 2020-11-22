using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
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

        public void GhostCrafterCrafingStarted(GameObject crafter, TechType techType, float duration)
        {
            NitroxId crafterId = NitroxEntity.GetId(crafter);
            GhostCrafterBeginCrafting ghostCrafterBeginCrafting = new GhostCrafterBeginCrafting(crafterId, techType.ToDto(), duration);
            packetSender.Send(ghostCrafterBeginCrafting);
        }

        public void GhostCrafterItemPickedUp(GameObject gameObject, TechType techType)
        {
            NitroxId crafterId = NitroxEntity.GetId(gameObject);
            GhostCrafterItemPickup ghostCrafterItemPickup = new GhostCrafterItemPickup(crafterId, techType.ToDto());
            packetSender.Send(ghostCrafterItemPickup);
        }
    }
}
