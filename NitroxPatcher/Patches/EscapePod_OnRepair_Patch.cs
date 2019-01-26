using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    public class EscapePod_OnRepair_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(EscapePod);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnRepair", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(EscapePod __instance)
        {
            NitroxServiceLocator.LocateService<EscapePodManager>().OnRepairedByMe(__instance);
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
