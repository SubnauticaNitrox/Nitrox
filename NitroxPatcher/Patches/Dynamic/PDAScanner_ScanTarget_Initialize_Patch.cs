using System.Reflection;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replace the internal PDAScanner use of UniqueIdentifier by NitroxEntity.
/// </summary>
public sealed partial class PDAScanner_ScanTarget_Initialize_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PDAScanner.ScanTarget t) => t.Initialize(default));

    public static void Postfix(PDAScanner.ScanTarget __instance)
    {
        // We only want to set the uid if the target is supposed to have an uid (hasUID)
        if (__instance.hasUID && __instance.gameObject.TryGetNitroxId(out NitroxId entityId))
        {
            __instance.uid = entityId.ToString();
        }
    }
}
