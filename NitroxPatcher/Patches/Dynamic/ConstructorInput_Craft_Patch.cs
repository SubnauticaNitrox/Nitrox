using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class ConstructorInput_Craft_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ConstructorInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Craft", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(ConstructorInput __instance, TechType techType, float duration)
        {
            NitroxServiceLocator.LocateService<MobileVehicleBay>().BeginCrafting(__instance.constructor.gameObject, techType, duration);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
