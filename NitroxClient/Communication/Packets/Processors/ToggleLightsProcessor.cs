using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class ToggleLightsProcessor : IClientPacketProcessor<Nitrox.Model.Subnautica.Packets.ToggleLights>
{
    public Task Process(ClientProcessorContext context, Nitrox.Model.Subnautica.Packets.ToggleLights packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject gameObject))
        {
            Log.Warn($"[{nameof(ToggleLightsProcessor)}] Could not find entity with id: {packet.Id}.");
            return Task.CompletedTask;
        }
        ToggleLights toggleLights = gameObject.RequireComponent<ToggleLights>();

        if (packet.IsOn == toggleLights.GetLightsActive())
        {
            return Task.CompletedTask;
        }

        using (PacketSuppressor<Nitrox.Model.Subnautica.Packets.ToggleLights>.Suppress())
        using (FMODSystem.SuppressSendingSounds())
        {
            toggleLights.SetLightsActive(packet.IsOn);
        }
        return Task.CompletedTask;
    }
}
