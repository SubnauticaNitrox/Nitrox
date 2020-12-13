using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
{
    class ToggleLightsProcessor : ClientPacketProcessor<Model.Packets.ToggleLights>
    {
        private readonly IPacketSender packetSender;

        public ToggleLightsProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }
        public override void Process(Model.Packets.ToggleLights packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
            ToggleLights toggleLights = gameObject.GetComponent<ToggleLights>();
            if (toggleLights == null)
            {
                toggleLights = gameObject.RequireComponentInChildren<ToggleLights>();
            }

            if (packet.IsOn != toggleLights.GetLightsActive())
            {
                using (packetSender.Suppress<Model.Packets.ToggleLights>())
                {
                    toggleLights.SetLightsActive(packet.IsOn);
                }
            }
        }
    }
}
