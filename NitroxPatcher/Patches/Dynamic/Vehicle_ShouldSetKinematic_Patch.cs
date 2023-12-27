using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Vehicle_ShouldSetKinematic_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Vehicle t) => t.ShouldSetKinematic());

    public static bool Prefix(Vehicle __instance, ref bool __result)
    {
        if (!__instance.GetRecentlyUndocked() && __instance.TryGetComponent(out MultiplayerMovementController movementController) && movementController.Receiving)
        {
            __result = false;
            return false;
        }
        return true;
    }
}
