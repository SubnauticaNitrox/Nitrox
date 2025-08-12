using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.GameLogic.FMOD;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class ToggleLightsProcessor : ClientPacketProcessor<NitroxModel.Packets.ToggleLights>
{
    private readonly FMODWhitelist fmodWhitelist;

    public ToggleLightsProcessor(FMODWhitelist fmodWhitelist)
    {
        this.fmodWhitelist = fmodWhitelist;
    }

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
    }
}
