using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

// Fixes a very annoying vanilla game bug where the Cyclops will appear
// to be leaking from the inside even though there is no actual damage.
public sealed partial class CyclopsExternalDamageManager_UpdateOvershield_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsExternalDamageManager t) => t.UpdateOvershield());

    public static void Postfix(CyclopsExternalDamageManager __instance)
    {
        __instance.ToggleLeakPointsBasedOnDamage();
    }
}
