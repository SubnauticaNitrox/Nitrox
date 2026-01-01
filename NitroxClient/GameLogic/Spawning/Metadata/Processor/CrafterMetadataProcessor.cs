using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
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
            EnsureCrafterReset(gameObject, crafterLogic);
        }
        else
        {
            SpawnItemInCrafter(gameObject, crafterLogic, metadata);
        }
    }

    private static void EnsureCrafterReset(GameObject gameObject, CrafterLogic crafterLogic)
    {
        crafterLogic.ResetCrafter();
        SetCrafterState(gameObject, false);
    }

    private static void SpawnItemInCrafter(GameObject gameObject, CrafterLogic crafterLogic, CrafterMetadata metadata)
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
            SetCrafterState(gameObject, true);
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

    private static void SetCrafterState(GameObject gameObject, bool crafting)
    {
        if (TryFindGhostCrafter(gameObject, out GhostCrafter ghostCrafter))
        {
            ghostCrafter.state = crafting;
        }
    }

    private static bool TryFindGhostCrafter(GameObject gameObject, out GhostCrafter ghostCrafter)
    {
        if (gameObject.TryGetComponentInChildren(out ghostCrafter, true))
        {
            return true;
        }

        if (gameObject.TryGetComponent(out CrafterLogic crafterLogic))
        {
            Base parentBase = gameObject.GetComponentInParent<Base>();
            if (parentBase)
            {
                foreach (GhostCrafter crafter in parentBase.GetComponentsInChildren<GhostCrafter>(true))
                {
                    if (crafter._logic == crafterLogic)
                    {
                        ghostCrafter = crafter;
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
