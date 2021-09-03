using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Constructable_Deconstruct_Patch : NitroxPatch, IDynamicPatch
    {
#if SUBNAUTICA
        public static readonly MethodInfo TARGET_METHOD = typeof(Constructable).GetMethod("Deconstruct");
#elif BELOWZERO
        public static readonly MethodInfo TARGET_METHOD = typeof(Constructable).GetMethod("DeconstructAsync");
#endif
#if SUBNAUTICA
        public static void Postfix(Constructable __instance, bool __result)
        {
            if (__result && __instance.constructedAmount <= 0f)
#elif BELOWZERO
        public static void Postfix(Constructable __instance, TaskResult<bool> result)
        {
            //TODO: Check this is actually valid code
            Log.Debug($"Deconstruct result from async is {result} {result.Get()}and is true {result.Equals(true)}");
            if (result.Get() && __instance.constructedAmount <= 0f)
#endif
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
