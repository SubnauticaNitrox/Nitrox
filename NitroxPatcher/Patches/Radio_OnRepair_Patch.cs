using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    public class Radio_OnRepair_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Radio);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnRepair", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(Radio __instance)
        {
            NitroxServiceLocator.LocateService<EscapePodManager>().OnRadioRepairedByMe(__instance);
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
