using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class CrafterLogic_OnTryPickupSingle_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CrafterLogic);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("TryPickupSingle", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(CrafterLogic __instance, bool __result, TechType techType)
        {
            __result = NitroxServiceLocator.LocateService<Crafting>().CrafterLogic_Pre_TryPickupSingle(__instance.gameObject, techType);
            return __result;
        }

        public static void Postfix(CrafterLogic __instance, bool __result, TechType techType)
        {
            NitroxServiceLocator.LocateService<Crafting>().CrafterLogic_Post_TryPickupSingle(__instance.gameObject, __result, techType);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }

   

}
