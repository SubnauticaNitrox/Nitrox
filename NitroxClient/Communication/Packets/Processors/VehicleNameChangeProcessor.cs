using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleNameChangeProcessor : ClientPacketProcessor<VehicleNameChange>
    {
        private readonly IPacketSender packetSender;

        public VehicleNameChangeProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(VehicleNameChange namePacket)
        {
            GameObject target = NitroxEntity.RequireObjectFrom(namePacket.Id);
            SubNameInput subNameInput = target.RequireComponentInChildren<SubNameInput>();

            using (packetSender.Suppress<VehicleNameChange>())
            {
                subNameInput.OnNameChange(namePacket.Name);
            }
        }
    }
}
