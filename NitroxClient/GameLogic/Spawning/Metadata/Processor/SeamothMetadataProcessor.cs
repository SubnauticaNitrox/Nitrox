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
    public SeamothMetadataProcessor(LiveMixinManager liveMixinManager) : base(liveMixinManager)
    { }

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

        using (FMODSystem.SuppressSendingSounds())
        {
            seamoth.toggleLights.SetLightsActive(lightsOn);
        }
    }
}
