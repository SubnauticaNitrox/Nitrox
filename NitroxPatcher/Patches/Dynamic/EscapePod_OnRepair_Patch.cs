using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EscapePod_OnRepair_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePod t) => t.OnRepair());

    public static void Prefix(EscapePod __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id) &&
            Resolve<EntityMetadataManager>().TryExtract(__instance, out EntityMetadata metadata))
        {
            Resolve<Entities>().BroadcastMetadataUpdate(id, metadata);
        }
    }
}
