using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Client
{
    public class Fabricator_OnCraftingBegin_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Fabricator);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnCraftingBegin", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(Fabricator __instance, TechType techType, float duration)
        {
            Multiplayer.Logic.Crafting.FabricatorCrafingStarted(__instance.gameObject, techType, duration);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
