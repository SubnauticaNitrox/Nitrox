using NitroxClient.Communication;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.GameLogic.Spawning.Metadata.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

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

        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            SetLights(seamoth, metadata.LightsOn);
        }

        base.ProcessMetadata(gameObject, metadata);
    }

    private void SetLights(SeaMoth seamoth, bool lightsOn)
    {
        using (FMODSystem.SuppressSounds())
        {
            seamoth.toggleLights.SetLightsActive(lightsOn);
        }
    }
}
