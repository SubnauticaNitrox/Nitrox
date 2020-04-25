using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class ConstructorInput_Craft_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(ConstructorInput).GetMethod("Craft", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(ConstructorInput __instance, TechType techType, float duration)
        {
            NitroxServiceLocator.LocateService<MobileVehicleBay>().BeginCrafting(__instance.constructor.gameObject, techType, duration);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
