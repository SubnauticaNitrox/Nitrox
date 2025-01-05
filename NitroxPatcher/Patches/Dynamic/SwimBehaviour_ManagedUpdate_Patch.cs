using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents <see cref="SwimBehaviour.ManagedUpdate"/> from happening on remotely controlled fishes (so their remote trajectory isn not modified)
/// </summary>
public sealed partial class SwimBehaviour_ManagedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SwimBehaviour t) => t.ManagedUpdate());

    public static bool Prefix(SwimBehaviour __instance)
    {
        return !__instance.GetComponent<RemotelyControlled>();
    }
}
