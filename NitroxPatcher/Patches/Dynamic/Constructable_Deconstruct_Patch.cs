using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Constructable_Deconstruct_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Constructable t) => t.Deconstruct());

        public static void Postfix(Constructable __instance, bool __result)
        {
            if (__result && __instance.constructedAmount <= 0f)
            {
                NitroxServiceLocator.LocateService<Building>().DeconstructionComplete(__instance.gameObject);
            }
            else if (!__instance._constructed && __instance.constructedAmount > 0)
            {
                NitroxServiceLocator.LocateService<Building>().ChangeConstructionAmount(__instance.gameObject, __instance.constructedAmount);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
