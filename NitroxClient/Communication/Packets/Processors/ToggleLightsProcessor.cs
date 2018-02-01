using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class ToggleLightsProcessor : ClientPacketProcessor<NitroxModel.Packets.ToggleLights>
    {
        private readonly IPacketSender packetSender;

        public ToggleLightsProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }
        public override void Process(NitroxModel.Packets.ToggleLights packet)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.Guid);
            ToggleLights toggleLights = gameObject.GetComponent<ToggleLights>();

            if (toggleLights == null)
            {
                toggleLights = gameObject.RequireComponentInChildren<ToggleLights>();
            }

            if (packet.IsOn != toggleLights.GetLightsActive())
            {
                using (packetSender.Suppress<NitroxModel.Packets.ToggleLights>())
                {
                    toggleLights.SetLightsActive(packet.IsOn);
                }
            }
        }
    }
}
