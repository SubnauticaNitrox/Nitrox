using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    public class Fabricator_OnCraftingBegin_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Fabricator);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnCraftingBegin", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(Fabricator __instance, TechType techType, float duration)
        {
            NitroxServiceLocator.LocateService<Crafting>().Fabricator_Post_OnCraftingBegin(__instance.gameObject, techType, duration);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }

    public class Fabricator_OnCraftingEnd_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Fabricator);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnCraftingEnd", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(Fabricator __instance)
        {
            return NitroxServiceLocator.LocateService<Crafting>().Fabricator_Pre_OnCraftingEnd(__instance.gameObject);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }

    }
}
