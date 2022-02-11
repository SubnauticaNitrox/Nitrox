using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
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
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
            ToggleLights toggleLights = gameObject.GetComponent<ToggleLights>();
            if (!toggleLights)
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
