using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class FabricatorItemPickupProcessor : ClientPacketProcessor<FabricatorItemPickup>
    {
        private PacketSender packetSender;

        public FabricatorItemPickupProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(FabricatorItemPickup packet)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.FabricatorGuid);
            CrafterLogic crafterLogic = gameObject.RequireComponentInChildren<CrafterLogic>(true);
                        
            if(crafterLogic.numCrafted > 0)
            {
                crafterLogic.numCrafted--;

                if(crafterLogic.numCrafted == 0)
                {
                    crafterLogic.Reset();
                } 
            }
        }
    }
}
