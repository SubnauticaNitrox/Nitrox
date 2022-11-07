using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class ToggleLightsProcessor : ClientPacketProcessor<NitroxModel.Packets.ToggleLights>
    {
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
                using (FMODSystem.SuppressSounds())
                {
                    toggleLights.SetLightsActive(packet.IsOn);
                }
            }
        }
    }
}
