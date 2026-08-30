using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class FlareMetadataProcessor : EntityMetadataProcessor<FlareMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, FlareMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out Flare flare))
        {
            Log.Error($"[{nameof(FlareMetadataProcessor)}] Can't apply metadata to {gameObject} because it doesn't have a {nameof(Flare)} component");
            return;
        }
        
        flare.hasBeenThrown = metadata.HasBeenThrown;

        if (metadata.FlareActivateTime.HasValue)
        {
            flare.flareActivateTime = metadata.FlareActivateTime.Value;
            flare.flareActiveState = true;
            // From Flare.OnDrop
            flare.useRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            // Calculate current energy accounting for time elapsed since extraction
            float currentTime = DayNightCycle.main.timePassedAsFloat;
            float timeSinceActivation = currentTime - metadata.FlareActivateTime.Value;
            flare.energyLeft = Mathf.Max(metadata.EnergyLeft - timeSinceActivation, 0f);
            
            flare.GetComponent<WorldForces>().enabled = true;

            // From Flare.Awake but without the part disabling the light
            flare.capRenderer.enabled = true;
            if (flare.fxControl && !flare.fxIsPlaying)
            {
                flare.fxControl.Play(1);
                flare.fxIsPlaying = true;
                flare.light.enabled = true;
            }
        }
        else
        {
            flare.energyLeft = metadata.EnergyLeft;
        }
    }
}
