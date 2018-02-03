using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ConstructionAmountChangedProcessor : ClientPacketProcessor<ConstructionAmountChanged>
    {
        private readonly IPacketSender packetSender;

        public ConstructionAmountChangedProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(ConstructionAmountChanged amountChanged)
        {
            Log.Debug("Processing ConstructionAmountChanged " + amountChanged.Guid + " " + amountChanged.ConstructionAmount);

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
