using NitroxClient.Communication;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class SeamothMetadataProcessor : VehicleMetadataProcessor<SeamothMetadata>
{
    private readonly FMODWhitelist fmodWhitelist;

    public SeamothMetadataProcessor(LiveMixinManager liveMixinManager, FMODWhitelist fmodWhitelist) : base(liveMixinManager)
    {
        this.fmodWhitelist = fmodWhitelist;
    }

    public override void ProcessMetadata(GameObject gameObject, SeamothMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out SeaMoth seamoth))
        {
            Log.ErrorOnce($"[{nameof(SeamothMetadataProcessor)}] Could not find {nameof(SeaMoth)} on {gameObject}");
            return;
        }

        if (!gameObject.TryGetComponent(out SubName subName))
        {
            Log.ErrorOnce($"[{nameof(SeamothMetadataProcessor)}] Could not find {nameof(SubName)} on {gameObject}");
            return;
        }

        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            SetLights(seamoth, metadata.LightsOn);
            SetHealth(seamoth.gameObject, metadata.Health);
            SetNameAndColors(subName, metadata.Name, metadata.Colors);
        }
    }

    private void SetLights(SeaMoth seamoth, bool lightsOn)
    {
        ToggleLights toggleLights = seamoth.toggleLights;

        if (lightsOn == toggleLights.GetLightsActive())
        {
            // Lights are already in the desired state, nothing to do.
            return;
        }

        // In the future, maybe do a patch for all ToggleLights.SetLightsActive() to handle all cases of lights toggling (i.e. powercell being inserted, etc.)
        using (FMODSystem.SuppressSendingSounds())
        {
            FMODAsset soundAsset;

            using (FMODSystem.SuppressSubnauticaSounds())
            {
                toggleLights.SetLightsActive(lightsOn);
            }

            if (lightsOn)
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
