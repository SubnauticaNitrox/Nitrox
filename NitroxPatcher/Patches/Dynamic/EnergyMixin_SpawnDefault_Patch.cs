using System.Collections;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class EnergyMixin_SpawnDefault_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EnergyMixin t) => t.SpawnDefaultAsync(default(float), default(TaskResult<bool>)));

        /*
         * When vehicles are spawned there is an async function that sets up the batteries
         * and energy mixins.  We'll skip this because it is managed by nitrox.
         */
        public static bool Prefix(EnergyMixin __instance, ref IEnumerator __result)
        {
            //Try to figure out if the default battery is spawned from a vehicle or cyclops
            if (__instance.gameObject.GetComponent<Vehicle>() ||
                __instance.gameObject.GetComponentInParent<Vehicle>() ||
                __instance.gameObject.GetComponentInParent<SubRoot>())
            {
                __result = nop();
                return false;
            }

            return true;
        }

        private static IEnumerator nop()
        {
            yield return null;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
