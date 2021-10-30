using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Radio_OnRepair_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Radio t) => t.OnRepair());

        public static bool Prefix(Radio __instance)
        {
            NitroxServiceLocator.LocateService<EscapePodManager>().OnRadioRepairedByMe(__instance);
            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
