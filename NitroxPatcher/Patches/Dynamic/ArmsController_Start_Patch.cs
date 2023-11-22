using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ArmsController_Start_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method((ArmsController t) => t.Start());

    public static void Postfix(ArmsController __instance)
    {
        __instance.Reconfigure(null);
    }
}
