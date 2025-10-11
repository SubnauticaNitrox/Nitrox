using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.DataStructures.Util;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Syncs metadata when a fish changes its breeding state if the local player is simulating the said fish.
/// </summary>
public sealed partial class WaterParkCreature_ResetBreedTime_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((WaterParkCreature t) => t.ResetBreedTime());

    public static void Postfix(WaterParkCreature __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId creatureId) ||
            !Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return;
        }

        Optional<WaterParkCreatureMetadata> metadata = Resolve<WaterParkCreatureMetadataExtractor>().Extract(__instance);
        if (metadata.HasValue)
        {
            Resolve<Entities>().BroadcastMetadataUpdate(creatureId, metadata.Value);
        }
    }
}
