using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    class GhostCrafter_OnHandHover_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(GhostCrafter);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHandHover", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(GhostCrafter __instance, GUIHand hand)
        {
            NitroxServiceLocator.LocateService<Crafting>().GhostCrafter_Post_OnHandHover(__instance.gameObject, hand);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }

    class GhostCrafter_OnHandClick_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(GhostCrafter);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(GhostCrafter __instance, GUIHand hand)
        {
            return NitroxServiceLocator.LocateService<Crafting>().GhostCrafter_Pre_OnHandClick(__instance.gameObject, hand);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }

    class GhostCrafter_OnOpenedChanged_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(GhostCrafter);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnOpenedChanged", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(GhostCrafter __instance, bool opened)
        {
            return NitroxServiceLocator.LocateService<Crafting>().GhostCrafter_Pre_OnOpenedChanged(__instance.gameObject, opened);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
