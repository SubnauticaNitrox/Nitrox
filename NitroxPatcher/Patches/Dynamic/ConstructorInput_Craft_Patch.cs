using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class ConstructorInput_Craft_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ConstructorInput t) => t.Craft(default(TechType), default(float)));

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
