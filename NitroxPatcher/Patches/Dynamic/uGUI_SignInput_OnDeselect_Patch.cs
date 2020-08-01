using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class uGUI_SignInput_OnDeselect_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(uGUI_SignInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnDeselect", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(uGUI_SignInput __instance)
        {
            NitroxServiceLocator.LocateService<Signs>().Changed(__instance);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
