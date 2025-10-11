using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Radio_OnRepair_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Radio t) => t.OnRepair());

    public static void Prefix(Radio __instance)
    {
        if (__instance.TryGetComponentInParent(out EscapePod pod) &&
            pod.TryGetIdOrWarn(out NitroxId id) &&
            Resolve<EntityMetadataManager>().TryExtract(pod, out EntityMetadata metadata))
        {
            Resolve<Entities>().BroadcastMetadataUpdate(id, metadata);
        }
    }
}
