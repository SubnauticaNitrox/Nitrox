using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Fire_Douse_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Fire t) => t.Douse(default(float)));

    public static void Postfix(Fire __instance, float amount)
    {
        if (!__instance.livemixin.IsAlive() || __instance.IsExtinguished())
        {
            Resolve<Fires>().OnDouse(__instance, 10000);
        }
        else
        {
            Resolve<Fires>().OnDouse(__instance, amount);
        }
    }
}
