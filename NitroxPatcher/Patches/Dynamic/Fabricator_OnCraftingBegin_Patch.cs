using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Fabricator_OnCraftingBegin_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Fabricator);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnCraftingBegin", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(Fabricator __instance, TechType techType, float duration)
        {
            NitroxServiceLocator.LocateService<Crafting>().FabricatorCrafingStarted(__instance.gameObject, techType, duration);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
