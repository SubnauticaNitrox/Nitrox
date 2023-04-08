using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// In the base game, subnautica uses a script to freeze all rigid bodies when they get out of distance.  However, this script
/// also messes with interpolation of objects - especially those being driven by other players.  We disable the functionality
/// when the Cyclops is fully loaded because there are validations that fail if removing it earlier (i.e. exceptions in code).
/// </summary>
public class VFXConstructing_WakeUpSubmarine_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((VFXConstructing t) => t.WakeUpSubmarine());

    public static void Postfix(VFXConstructing __instance)
    {
        FreezeRigidbodyWhenFar freezer = __instance.GetComponent<FreezeRigidbodyWhenFar>();

        if (freezer)
        {
            freezer.enabled = false;
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
