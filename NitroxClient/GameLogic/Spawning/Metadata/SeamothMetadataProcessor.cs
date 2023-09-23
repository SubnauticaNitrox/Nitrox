using NitroxClient.Communication;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class SeamothMetadataProcessor : GenericEntityMetadataProcessor<SeamothMetadata>
{
    private readonly LiveMixinManager liveMixinManager;
    private readonly float lightsSoundRadius;

    public SeamothMetadataProcessor(LiveMixinManager liveMixinManager, FMODWhitelist fmodWhitelist)
    {
        this.liveMixinManager = liveMixinManager;

        fmodWhitelist.IsWhitelisted("event:/sub/seamoth/seamoth_light_on", out lightsSoundRadius);
    }

    public override void ProcessMetadata(GameObject gameObject, SeamothMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out SeaMoth seamoth))
        {
            Log.Error($"Could not find seamoth on {gameObject.name}");
            return;
        }

        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            SetLights(seamoth, metadata.LightsOn);
            SetHealth(seamoth, metadata.Health);
        }
    }

    private void SetLights(SeaMoth seamoth, bool lightsOn)
    {
        ToggleLights toggleLights = seamoth.toggleLights;

        if (toggleLights.lightsActive == lightsOn)
        {
            return;
        }

        using (FMODSystem.SuppressSubnauticaSounds())
        {
            toggleLights.SetLightsActive(lightsOn);
        }

        FMODEmitterController.PlayEventOneShot(lightsOn ? toggleLights.lightsOnSound.asset : toggleLights.lightsOffSound.asset, lightsSoundRadius, toggleLights.transform.position);
    }

    private void SetHealth(SeaMoth seamoth, float health)
    {
        liveMixinManager.SyncRemoteHealth(seamoth.liveMixin, health);
    }
}
