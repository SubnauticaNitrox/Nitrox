using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
///     Syncs metadata when a fish changes its breeding state if the local player is simulating the said fish.
/// </summary>
internal sealed partial class WaterParkCreature_ResetBreedTime_Patch : NitroxPatch, IDynamicPatch
{
    private static SimulationOwnership simulationOwnership;
    private static Entities entities;
    private static WaterParkCreatureMetadataExtractor waterParkCreatureMetadataExtractor;
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((WaterParkCreature t) => t.ResetBreedTime());

    public WaterParkCreature_ResetBreedTime_Patch(SimulationOwnership so, Entities e, WaterParkCreatureMetadataExtractor wpcme)
    {
        simulationOwnership = so ?? throw new ArgumentNullException(nameof(so));
        entities = e ?? throw new ArgumentNullException(nameof(e));
        waterParkCreatureMetadataExtractor = wpcme ?? throw new ArgumentNullException(nameof(wpcme));
    }

    public static void Postfix(WaterParkCreature __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId creatureId) ||
            !simulationOwnership.HasAnyLockType(creatureId))
        {
            return;
        }

        Optional<WaterParkCreatureMetadata> metadata = waterParkCreatureMetadataExtractor.Extract(__instance);
        if (metadata.HasValue)
        {
            entities.BroadcastMetadataUpdate(creatureId, metadata.Value);
        }
    }
}
