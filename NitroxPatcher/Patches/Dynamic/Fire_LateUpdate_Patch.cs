using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Fire_LateUpdate_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Fire t) => t.LateUpdate());

    public static void Prefix(Fire __instance)
    {
        Resolve<Fires>().OnUpdate(__instance);
    }
}
