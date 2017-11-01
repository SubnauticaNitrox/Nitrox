using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleNameChangeProcessor : ClientPacketProcessor<VehicleNameChange>
    {
        private readonly PacketSender packetSender;

        public VehicleNameChangeProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(VehicleNameChange namePacket)
        {
            GameObject target = GuidHelper.RequireObjectFrom(namePacket.Guid);
            SubNameInput subNameInput = target.RequireComponentInChildren<SubNameInput>();

            using (packetSender.Suppress<VehicleNameChange>())
            {
                subNameInput.OnNameChange(namePacket.Name);
            }
        }
    }
}
