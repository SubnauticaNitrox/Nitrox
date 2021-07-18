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
        public override void Process(GhostCrafterBeginCrafting packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.GhostCrafterId);
            GhostCrafter ghostCrafter = gameObject.RequireComponentInChildren<GhostCrafter>(true);

            float buildDuration = packet.Duration + 0.2f; // small increase to prevent this player from swiping item from remote player

            ghostCrafter.logic.Craft(packet.TechType.ToUnity(), buildDuration);
        }
    }
}
