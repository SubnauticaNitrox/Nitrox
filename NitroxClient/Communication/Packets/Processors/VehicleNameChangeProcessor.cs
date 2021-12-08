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
            SubNameInput subNameInput;

            if (namePacket.ParentId.HasValue)
            {
                GameObject moonpool = NitroxEntity.RequireObjectFrom(namePacket.ParentId.Value);
                GameObject baseCell = moonpool.RequireComponentInParent<BaseCell>().gameObject;

                subNameInput = baseCell.GetComponentInChildren<SubNameInput>();
            }
            else
            {
                GameObject vehicleObject = NitroxEntity.RequireObjectFrom(namePacket.VehicleId);
                subNameInput = vehicleObject.GetComponentInChildren<SubNameInput>();
            }

            using (packetSender.Suppress<VehicleNameChange>())
            {
                if (subNameInput && subNameInput.target)
                {
                    // OnColorChange calls these two methods, in order to update the vehicle name on the ingame panel:
                    subNameInput.target.SetName(namePacket.Name);
                    subNameInput.SetName(namePacket.Name);
                }
                else
                {
                    Log.Error($"[VehicleNameChangeProcessor] SubNameInput or targeted SubName is null with {namePacket}.");
                }
            }
        }
    }
}
