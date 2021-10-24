using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Fire_Douse_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Fire t) => t.Douse(default(float)));

        public static void Postfix(Fire __instance, float amount)
        {
            if (!__instance.livemixin.IsAlive() || __instance.IsExtinguished())
            {
                NitroxServiceLocator.LocateService<Fires>().OnDouse(__instance, 10000);
            }
            else
            {
                NitroxServiceLocator.LocateService<Fires>().OnDouse(__instance, amount);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
