using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Subnautica enables high precision physics any time the local player is in a moving cyclops and is not piloting it.
/// We force a higher fixed timestep as long as the local player is inside of a Cyclops to avoid it stuttering.
/// </summary>
public sealed partial class Player_RequiresHighPrecisionPhysics_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.RequiresHighPrecisionPhysics());

    public static bool Prefix(Player __instance, ref bool __result)
    {
        if (__instance.currentSub && __instance.currentSub.isCyclops)
        {
            __result = true;
            return false;
        }

        return true;
    }
}
