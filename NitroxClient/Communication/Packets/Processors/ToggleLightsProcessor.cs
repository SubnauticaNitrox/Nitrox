using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class ToggleLightsProcessor : ClientPacketProcessor<Nitrox.Model.Subnautica.Packets.ToggleLights>
{
    public ToggleLightsProcessor()
    { }

    public override void Process(Nitrox.Model.Subnautica.Packets.ToggleLights packet)
    {
        GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
        ToggleLights toggleLights = gameObject.RequireComponent<ToggleLights>();

        if (packet.IsOn == toggleLights.GetLightsActive())
        {
            return;
        }

        using (PacketSuppressor<Nitrox.Model.Subnautica.Packets.ToggleLights>.Suppress())
        using (FMODSystem.SuppressSendingSounds())
        {
            toggleLights.SetLightsActive(packet.IsOn);
        }
    }
}
