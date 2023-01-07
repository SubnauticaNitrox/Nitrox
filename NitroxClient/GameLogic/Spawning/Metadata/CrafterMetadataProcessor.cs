using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class CrafterMetadataProcessor : GenericEntityMetadataProcessor<CrafterMetadata>
{
    // small increase to prevent this player from swiping item from remote player
    public const float ANTI_GRIEF_DURATION_BUFFER = 0.2f;

    public override void ProcessMetadata(GameObject gameObject, CrafterMetadata metadata)
    {
        if (metadata.TechType == null)
        {
            EnsureCrafterReset(gameObject);
        }
        else
        {
            SpawnItemInCrafter(gameObject, metadata);
        }
    }

    private void EnsureCrafterReset(GameObject gameObject)
    {
        CrafterLogic crafterLogic = gameObject.RequireComponentInChildren<CrafterLogic>(true);
        crafterLogic.ResetCrafter();
    }

    private void SpawnItemInCrafter(GameObject gameObject, CrafterMetadata metadata)
    {
        GhostCrafter ghostCrafter = gameObject.RequireComponentInChildren<GhostCrafter>(true);

        float elapsedFromStart = (DayNightCycle.main.timePassedAsFloat - metadata.StartTime);

        // If a craft started way in the past, set duration to 0.01 (the craft function will not work with 0)
        // Keeping track of both the duration and start time allows us to solve use-cases such as reloading
        // when an item is being crafted or not picked up yet. 
        float duration = Mathf.Max(metadata.Duration - elapsedFromStart + ANTI_GRIEF_DURATION_BUFFER, 0.01f);

        ghostCrafter.logic.Craft(metadata.TechType.ToUnity(), duration);
    }
}
