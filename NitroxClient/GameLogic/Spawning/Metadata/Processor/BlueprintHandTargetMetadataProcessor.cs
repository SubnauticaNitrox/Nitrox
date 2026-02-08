using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

/// <summary>
/// Processes DataBox (BlueprintHandTarget) metadata updates from other players.
/// When another player opens a DataBox, this applies the visual state change locally.
/// </summary>
public sealed class BlueprintHandTargetMetadataProcessor : EntityMetadataProcessor<BlueprintHandTargetMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, BlueprintHandTargetMetadata metadata)
    {
        BlueprintHandTarget blueprintHandTarget = gameObject.GetComponent<BlueprintHandTarget>();
        if (!blueprintHandTarget)
        {
            Log.Error($"[BlueprintHandTargetMetadataProcessor] No BlueprintHandTarget component found on {gameObject.name}");
            return;
        }

        // Skip if already in the target state
        if (blueprintHandTarget.used == metadata.Used)
        {
            return;
        }

        blueprintHandTarget.used = metadata.Used;

        if (metadata.Used)
        {
            // Trigger the animation if the DataBox has an animator
            if (!string.IsNullOrEmpty(blueprintHandTarget.animParam) && blueprintHandTarget.animator)
            {
                blueprintHandTarget.animator.SetBool(blueprintHandTarget.animParam, true);
            }

            // Disable the visual game object (the lid/door of the DataBox)
            if (blueprintHandTarget.disableGameObject)
            {
                blueprintHandTarget.disableGameObject.SetActive(false);
            }

            // Unregister from resource tracker to remove the scanner ping
            if (blueprintHandTarget.resourceTracker)
            {
                blueprintHandTarget.resourceTracker.OnBlueprintHandTargetUsed();
            }
        }
    }
}
