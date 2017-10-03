using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ConstructionAmountChangedProcessor : ClientPacketProcessor<ConstructionAmountChanged>
    {
        private PacketSender packetSender;

        public ConstructionAmountChangedProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(ConstructionAmountChanged amountChanged)
        {
            Console.WriteLine("Processing ConstructionAmountChanged " + amountChanged.Guid + " " + amountChanged.PlayerId + " " + amountChanged.ConstructionAmount);

            GameObject constructing = GuidHelper.RequireObjectFrom(amountChanged.Guid);            
            Constructable constructable = constructing.GetComponent<Constructable>();
            constructable.constructedAmount = amountChanged.ConstructionAmount;

            using (packetSender.Suppress<ConstructionAmountChanged>())
            {
                constructable.Construct();
            }
        }
    }
}
