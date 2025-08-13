using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.GameLogic.FMOD;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class ToggleLightsProcessor : ClientPacketProcessor<NitroxModel.Packets.ToggleLights>
{
    public ToggleLightsProcessor()
    { }

    public override void Process(NitroxModel.Packets.ToggleLights packet)
    {
        GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
        ToggleLights toggleLights = gameObject.RequireComponent<ToggleLights>();

        if (packet.IsOn == toggleLights.GetLightsActive())
        {
            return;
        }

        using (PacketSuppressor<NitroxModel.Packets.ToggleLights>.Suppress())
        using (FMODSystem.SuppressSendingSounds())
        {
            toggleLights.SetLightsActive(packet.IsOn);
        }
    }
}
