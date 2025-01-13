using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EscapePod_OnRepair_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePod t) => t.OnRepair());

    public static void Prefix(EscapePod __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id) &&
            __instance.radioSpawner.spawnedObj.TryGetComponent(out Radio radio))
        {
            Resolve<Entities>().BroadcastMetadataUpdate(id, new EscapePodMetadata(true, radio.liveMixin.IsFullHealth()));
        }
    }
}
