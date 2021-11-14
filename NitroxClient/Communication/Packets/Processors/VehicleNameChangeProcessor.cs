using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;
using NitroxModel.Logger;

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


            if (namePacket.ParentId.HasValue)
            {
                GameObject moonpool = NitroxEntity.RequireObjectFrom(namePacket.ParentId.Value);
                GameObject baseCell = moonpool.RequireComponentInParent<BaseCell>().gameObject;

                subNameInput = baseCell.RequireComponentInChildren<SubNameInput>();
                subNameTarget = subNameInput.target;

            }
            else
            {
                subNameInput = vehicleObject.RequireComponentInChildren<SubNameInput>();
                subNameTarget = subNameInput.target;
            }

            if (subNameInput && subNameTarget)
            {
                if (vehicleObject.GetComponent<Vehicle>())
                {
                    // OnColorChange calls these two methods, in order to update the vehicle name on the ingame panel:
                    subNameTarget.SetName(namePacket.Name);
                    subNameInput.SetName(namePacket.Name);

                }
                else
                {
                    Log.Error($"[VehicleNameChangeProcessor] The GameObject {vehicleObject.name} doesn't have a Vehicle component");
                }
            }
            else
            {
                Log.Error($"[VehicleNameChangeProcessor] SubNameInput or targeted SubName is null.");
            }
        }
    }
}
