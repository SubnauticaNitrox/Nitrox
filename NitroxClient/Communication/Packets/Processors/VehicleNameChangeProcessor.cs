using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;
using NitroxModel.DataStructures.Util;

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
            GameObject vehicleObject = NitroxEntity.RequireObjectFrom(namePacket.VehicleId);
            SubNameInput subNameInput = null;
            SubName subNameTarget = null;

            if (vehicleObject.GetComponent<Vehicle>())
            {
                if (namePacket.ParentId.HasValue)
                {
                    GameObject moonpool = NitroxEntity.RequireObjectFrom(namePacket.ParentId.Value);
                    GameObject baseCell = moonpool.RequireComponentInParent<BaseCell>().gameObject;

                    subNameInput = baseCell.RequireComponentInChildren<SubNameInput>();
                    subNameTarget = vehicleObject.RequireComponentInChildren<SubName>();
                }
            }
            else
            {
                subNameInput = vehicleObject.RequireComponentInChildren<SubNameInput>();
                subNameTarget = subNameInput.target;
            }

            if (subNameInput != null && subNameTarget != null)
            {
                using (packetSender.Suppress<VehicleColorChange>())
                {
                    // OnColorChange calls these two methods, in order to update the vehicle color and the color+text on the ingame panel, respectively:
                    subNameTarget.SetName(namePacket.Name);
                    subNameInput.SetName(namePacket.Name);

                }
            }
        }
    }
}
