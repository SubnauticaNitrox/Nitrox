using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Applies the local Cyclops pilot's forward speed boost for the duration of a physics update.
/// </summary>
public sealed partial class SubControl_FixedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubControl t) => t.FixedUpdate());

    public static void Prefix(SubControl __instance, out float __state)
    {
        __state = VehicleSpeedBoost.ApplyTemporaryMultiplier(ref __instance.BaseForwardAccel, VehicleSpeedBoost.IsActive(__instance));
    }

    public static void Postfix(SubControl __instance, float __state)
    {
        VehicleSpeedBoost.Restore(ref __instance.BaseForwardAccel, __state);
    }
}
