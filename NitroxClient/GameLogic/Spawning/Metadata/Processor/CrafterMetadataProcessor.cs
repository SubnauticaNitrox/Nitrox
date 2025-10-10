using NitroxClient.Extensions;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class CrafterMetadataProcessor : EntityMetadataProcessor<CrafterMetadata>
{
    // small increase to prevent this player from swiping item from remote player
    public const float ANTI_GRIEF_DURATION_BUFFER = 0.2f;

    public override void ProcessMetadata(GameObject gameObject, CrafterMetadata metadata)
    {
        CrafterLogic crafterLogic = gameObject.RequireComponentInChildren<CrafterLogic>(true);

        if (metadata.TechType == NitroxTechType.None || metadata.Amount == 0)
        {
            EnsureCrafterReset(crafterLogic);
        }
        else
        {
            SpawnItemInCrafter(crafterLogic, metadata);
        }
    }

    private static void EnsureCrafterReset(CrafterLogic crafterLogic)
    {
        crafterLogic.ResetCrafter();
    }

    private static void SpawnItemInCrafter(CrafterLogic crafterLogic, CrafterMetadata metadata)
    {
        float elapsedFromStart = DayNightCycle.main.timePassedAsFloat - metadata.StartTime;

        // If a craft started way in the past, set duration to 0.01 (the craft function will not work with 0)
        // Keeping track of both the duration and start time allows us to solve use-cases such as reloading
        // when an item is being crafted or not picked up yet. 
        float duration = Mathf.Max(metadata.Duration - elapsedFromStart + ANTI_GRIEF_DURATION_BUFFER, 0.01f);

        crafterLogic.linkedIndex = metadata.LinkedIndex;
        if (metadata.LinkedIndex == -1)
        {
            crafterLogic.Craft(metadata.TechType.ToUnity(), duration);
        }
        else
        {
            // Ensure craft is finished and has the right data
            crafterLogic.craftingTechType = metadata.TechType.ToUnity();
            crafterLogic.timeCraftingBegin = metadata.StartTime;
            crafterLogic.timeCraftingEnd = DayNightCycle.main.timePassedAsFloat;
            crafterLogic.NotifyChanged(crafterLogic.currentTechType);
            crafterLogic.NotifyProgress(1f);
        }
        // Override this value in case some of the crafted items were already picked up
        crafterLogic.numCrafted = metadata.Amount;
    }
}
