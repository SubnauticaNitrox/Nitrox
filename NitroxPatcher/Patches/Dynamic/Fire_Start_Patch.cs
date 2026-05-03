using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Fire_Start_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Fire t) => t.Start());

    public static void Postfix(Fire __instance)
    {
        Resolve<Fires>().OnCreate(__instance);
    }
}
