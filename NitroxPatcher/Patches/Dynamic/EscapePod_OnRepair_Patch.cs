using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class EscapePod_OnRepair_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(EscapePod);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnRepair", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(EscapePod __instance)
        {
            NitroxServiceLocator.LocateService<EscapePodManager>().OnRepairedByMe(__instance);
            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
