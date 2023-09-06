using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class ToggleLightsProcessor : ClientPacketProcessor<NitroxModel.Packets.ToggleLights>
{
    public override void Process(NitroxModel.Packets.ToggleLights packet)
    {
        GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
        ToggleLights toggleLights = gameObject.RequireComponentInChildren<ToggleLights>();

        if (packet.IsOn != toggleLights.GetLightsActive())
        {
            using (PacketSuppressor<NitroxModel.Packets.ToggleLights>.Suppress())
            {
                toggleLights.SetLightsActive(packet.IsOn);
            }
        }
    }
}
