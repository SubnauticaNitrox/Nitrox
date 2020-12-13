using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
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
