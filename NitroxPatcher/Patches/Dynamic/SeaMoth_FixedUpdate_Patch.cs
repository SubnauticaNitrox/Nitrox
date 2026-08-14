using System.Reflection;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Disables <see cref="SeaMoth.FixedUpdate"/> for not simulated Seamoths.
/// </summary>
public sealed partial class Seamoth_FixedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((SeaMoth t) => t.FixedUpdate());

    public static bool Prefix(SeaMoth __instance, out float __state)
    {
        __state = VehicleSpeedBoost.ApplyTemporaryMultiplier(ref __instance.forwardForce, VehicleSpeedBoost.IsActive(__instance));
        return !__instance.GetComponent<MovementReplicator>();
    }

    public static void Postfix(SeaMoth __instance, float __state)
    {
        VehicleSpeedBoost.Restore(ref __instance.forwardForce, __state);
    }
}
