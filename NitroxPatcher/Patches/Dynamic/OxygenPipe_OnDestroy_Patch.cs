using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Forces the destruction of an oxygen pipe to be "natural", for example when another player picks up an oxygen pipe, the parent will be notified of it.
/// </summary>
public sealed partial class OxygenPipe_OnDestroy_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((OxygenPipe t) => t.OnDestroy());

    public static void Prefix(OxygenPipe __instance)
    {
        IPipeConnection parent = __instance.GetParent();
        if (parent != null)
        {
            parent.RemoveChild(__instance);
        }
    }
}
