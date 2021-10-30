using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class GhostCrafter_OnCraftingBegin_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((GhostCrafter t) => t.OnCraftingBegin(default(TechType), default(float)));

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
