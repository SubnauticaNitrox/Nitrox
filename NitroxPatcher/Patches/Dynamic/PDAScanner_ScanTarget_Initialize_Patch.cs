using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replace the internal PDAScanner use of UniqueIdentifier by NitroxEntity.
/// </summary>
public class PDAScanner_ScanTarget_Initialize_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PDAScanner.ScanTarget t) => t.Initialize(default));

    public static void Postfix(PDAScanner.ScanTarget __instance)
    {
        // We only want to set the uid if the target is supposed to have an uid (hasUID)
        if (__instance.hasUID && NitroxEntity.TryGetEntityFrom(__instance.gameObject, out NitroxEntity entity))
        {
            __instance.uid = entity.Id.ToString();
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
