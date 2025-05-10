using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.GameLogic.FMOD;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class ToggleLightsProcessor : IClientPacketProcessor<NitroxModel.Networking.Packets.ToggleLights>
{
    private readonly FmodWhitelist fmodWhitelist;

    public ToggleLightsProcessor(FmodWhitelist fmodWhitelist)
    {
        this.fmodWhitelist = fmodWhitelist;
    }

    public Task Process(IPacketProcessContext context, NitroxModel.Networking.Packets.ToggleLights packet)
    {
        GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
        ToggleLights toggleLights = gameObject.RequireComponent<ToggleLights>();

        if (packet.IsOn == toggleLights.GetLightsActive())
        {
            return Task.CompletedTask;
        }

        using (PacketSuppressor<NitroxModel.Networking.Packets.ToggleLights>.Suppress())
        using (FMODSystem.SuppressSendingSounds())
        {
            using (FMODSystem.SuppressSubnauticaSounds())
            {
                toggleLights.SetLightsActive(packet.IsOn);
            }

            FMODAsset soundAsset;

            if (packet.IsOn)
            {
                soundAsset = toggleLights.lightsOnSound ? toggleLights.lightsOnSound.asset : toggleLights.onSound;
            }
            else
            {
                soundAsset = toggleLights.lightsOffSound ? toggleLights.lightsOffSound.asset : toggleLights.offSound;
            }

            if (soundAsset && fmodWhitelist.TryGetSoundData(soundAsset.path, out SoundData soundData))
            {
                FMODEmitterController.PlayEventOneShot(soundAsset, soundData.Radius, toggleLights.transform.position);
            }
        }
        return Task.CompletedTask;
    }
}
