using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Stalker_FindNest_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((Stalker t) => t.FindNest());

    public static bool Prefix(Stalker __instance)
    {
        if (__instance.TryGetNitroxId(out NitroxId creatureId)
            && Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return true;
        }

        return false;
    }

    public static void Postfix(Stalker __instance, bool __result)
    {
        // If the original method returned false, we don't want to do anything since leashPosition didn't change
        if (!__result)
        {
            return;
        }

        if (__instance.TryGetNitroxId(out NitroxId creatureId))
        {
            StayAtLeashPositionMetadata metadata = Resolve<StayAtLeashPositionMetadataExtractor>().Extract(__instance);
            Resolve<Entities>().BroadcastMetadataUpdate(creatureId, metadata);
        }
    }
}
