using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class SeaTreader_SetNextPathPoint_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaTreader t) => t.SetNextPathPoint());

    public static void Postfix(SeaTreader __instance)
    {
        if (__instance.TryGetNitroxId(out NitroxId creatureId) &&
            Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            SeaTreaderMetadata metadata = Resolve<SeaTreaderMetadataExtractor>().Extract(__instance);
            Resolve<Entities>().BroadcastMetadataUpdate(creatureId, metadata);
        }
    }
}
