using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.Extensions;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class CrafterMetadataProcessor : EntityMetadataProcessor<CrafterMetadata>
{
    // small increase to prevent this player from swiping item from remote player
    public const float ANTI_GRIEF_DURATION_BUFFER = 0.2f;
    private const float CRAFT_ENERGY_COST = 5f;

    private static readonly Dictionary<NitroxId, float> lastConsumedCraftStartById = [];

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
            ConsumeCraftPower(gameObject, crafterLogic, metadata);
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

    private static void ConsumeCraftPower(GameObject gameObject, CrafterLogic crafterLogic, CrafterMetadata metadata)
    {
        if (gameObject.TryGetNitroxId(out NitroxId crafterId))
        {
            if (lastConsumedCraftStartById.TryGetValue(crafterId, out float previousStart) && previousStart == metadata.StartTime)
            {
                return;
            }
            lastConsumedCraftStartById[crafterId] = metadata.StartTime;
        }

        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }

        PowerRelay powerRelay = crafterLogic.GetComponentInParent<PowerRelay>();
        if (powerRelay)
        {
            SubRoot subRoot = powerRelay.GetComponentInParent<SubRoot>();
            if (!subRoot || subRoot.isBase)
            {
                CrafterLogic.ConsumeEnergy(powerRelay, CRAFT_ENERGY_COST);
            }
        }
    }

    public static void MarkLocalCraftAccounted(NitroxId crafterId, float startTime)
    {
        if (crafterId != null)
        {
            lastConsumedCraftStartById[crafterId] = startTime;
        }
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

        if (!gameObject.TryGetComponent(out CrafterLogic crafterLogic))
        {
            return false;
        }

        Base parentBase = gameObject.GetComponentInParent<Base>();
        if (!parentBase)
        {
            return false;
        }

        foreach (GhostCrafter crafter in parentBase.GetComponentsInChildren<GhostCrafter>(true))
        {
            if (crafter._logic == crafterLogic)
            {
                ghostCrafter = crafter;
                return true;
            }
        }

        return false;
    }
}
