using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class GhostCrafter_OnCraftingBegin_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(GhostCrafter).GetMethod("OnCraftingBegin", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(GhostCrafter __instance, TechType techType, float duration)
        {
            NitroxServiceLocator.LocateService<Crafting>().GhostCrafterCrafingStarted(__instance.gameObject, techType, duration);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
