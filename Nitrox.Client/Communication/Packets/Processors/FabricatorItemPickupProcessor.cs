using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class FabricatorItemPickupProcessor : ClientPacketProcessor<FabricatorItemPickup>
    {
        private readonly IPacketSender packetSender;

        public FabricatorItemPickupProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(FabricatorItemPickup packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.FabricatorId);
            CrafterLogic crafterLogic = gameObject.RequireComponentInChildren<CrafterLogic>(true);

            if (crafterLogic.numCrafted > 0)
            {
                crafterLogic.numCrafted--;

                if (crafterLogic.numCrafted == 0)
                {
                    crafterLogic.Reset();
                }
            }
        }
    }
}
