using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Disables <see cref="FreezeRigidbodyWhenFar"/>'s behaviour.
/// </summary>
public sealed partial class FreezeRigidbodyWhenFar_Awake_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FreezeRigidbodyWhenFar t) => t.Awake());

    public static bool Prefix(FreezeRigidbodyWhenFar __instance)
    {
        __instance.enabled = false;
        return false;
    }
}
