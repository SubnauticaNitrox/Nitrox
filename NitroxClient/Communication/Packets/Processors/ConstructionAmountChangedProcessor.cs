using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
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

            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(amountChanged.Guid);

            if (opGameObject.IsPresent())
            {
                GameObject constructing = opGameObject.Get();
                Constructable constructable = constructing.GetComponent<Constructable>();
                constructable.constructedAmount = amountChanged.ConstructionAmount;

                using (packetSender.Suppress<ConstructionAmountChanged>())
                {
                    constructable.Construct();
                }
            }
        }
    }
}
