using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
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
            GameObject target = NitroxEntity.RequireObjectFrom(colorPacket.Id);
            SubNameInput subNameInput = target.RequireComponentInChildren<SubNameInput>();
            SubName subNameTarget = subNameInput.target;

            using (packetSender.Suppress<VehicleColorChange>())
            {
                // Switch to the currently selected tab:
                subNameInput.SetSelected(colorPacket.Index);

                // OnColorChange calls these two methods, in order to update the vehicle color and the color+text on the ingame panel, respectively:
                subNameTarget.SetColor(colorPacket.Index, colorPacket.HSB.ToUnity(), colorPacket.Color.ToUnity());
                subNameInput.SetColor(colorPacket.Index, colorPacket.Color.ToUnity());
            }
        }
    }
}
