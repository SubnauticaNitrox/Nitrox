using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches
{
    public class CyclopsMotorModeButton_OnClick_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsMotorModeButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnClick", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(CyclopsMotorModeButton __instance, out bool __state)
        {
            SubRoot cyclops = (SubRoot)__instance.ReflectionGet("subRoot");
            if (cyclops != null && cyclops == Player.main.currentSub)
            {
                CyclopsHelmHUDManager cyclops_HUD = cyclops.gameObject.RequireComponentInChildren<CyclopsHelmHUDManager>();
                // To show the Cyclops HUD every time "hudActive" have to be true. "hornObject" is a good indicator to check if the player piloting the cyclops.
                if ((bool)cyclops_HUD.ReflectionGet("hudActive"))
                {
                    __state = cyclops_HUD.hornObject.activeSelf;
                    return cyclops_HUD.hornObject.activeSelf;
                }
            }

            __state = false;
            return false;
        }

        public static void Postfix(CyclopsMotorModeButton __instance, bool __state)
        {
            if (__state)
            {
                SubRoot cyclops = (SubRoot)__instance.ReflectionGet("subRoot");
                string guid = GuidHelper.GetGuid(cyclops.gameObject);
                NitroxServiceLocator.LocateService<Cyclops>().BroadcastChangeEngineMode(guid, __instance.motorModeIndex);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
