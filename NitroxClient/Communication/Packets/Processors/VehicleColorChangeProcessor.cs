using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleColorChangeProcessor : ClientPacketProcessor<VehicleColorChange>
    {
        private readonly IPacketSender packetSender;

        public VehicleColorChangeProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(VehicleColorChange colorPacket)
        {
            SubNameInput subNameInput;

            if (colorPacket.ParentId.HasValue)
            {
                GameObject moonpool = NitroxEntity.RequireObjectFrom(colorPacket.ParentId.Value);
                GameObject baseCell = moonpool.RequireComponentInParent<BaseCell>().gameObject;

                subNameInput = baseCell.GetComponentInChildren<SubNameInput>();
            }
            else
            {
                if (!NitroxEntity.TryGetObjectFrom(colorPacket.VehicleId, out GameObject vehicleObject))
                {
                    return;
                }

                subNameInput = vehicleObject.GetComponentInChildren<SubNameInput>();
            }

            using (packetSender.Suppress<VehicleColorChange>())
            {
                if (subNameInput && subNameInput.target)
                {
                    // Switch to the currently selected tab
                    subNameInput.SetSelected(colorPacket.Index);

                    // OnColorChange calls these two methods, in order to update the vehicle color and the color+text on the ingame panel, respectively:
                    subNameInput.target.SetColor(colorPacket.Index, colorPacket.HSB.ToUnity(), colorPacket.Color.ToUnity());
                    subNameInput.SetColor(colorPacket.Index, colorPacket.Color.ToUnity());
                }
                else
                {
                    Log.Error($"[VehicleColorChangeProcessor] SubNameInput or targeted SubName is null with {colorPacket}.");
                }
            }
        }
    }
}
