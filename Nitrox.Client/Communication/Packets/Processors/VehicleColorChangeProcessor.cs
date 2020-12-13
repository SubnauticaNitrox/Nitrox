using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Helper;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
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
            SubName subNameTarget = (SubName)subNameInput.ReflectionGet("target");

            using (packetSender.Suppress<VehicleColorChange>())
            {
                // Switch to the currently selected tab:
                subNameInput.SetSelected(colorPacket.Index);

                // OnColorChange calls these two methods, in order to update the vehicle color and the color+text on the ingame panel, respectively:
                subNameTarget.SetColor(colorPacket.Index, colorPacket.HSB.ToUnity(), colorPacket.Color.ToUnity());
                subNameInput.ReflectionCall("SetColor", args: new object[] { colorPacket.Index, colorPacket.Color });
            }
        }
    }
}
