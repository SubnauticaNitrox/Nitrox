using Harmony;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Client
{
    public class CyclopsMotorModeButton_OnClick_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsMotorModeButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnClick", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(CyclopsMotorModeButton __instance)
        {
            SubRoot cyclops = (SubRoot)__instance.ReflectionGet("subRoot");
            if (cyclops != null)
            {
                CyclopsHelmHUDManager cyclops_HUD = cyclops.gameObject.RequireComponentInChildren<CyclopsHelmHUDManager>();
                return (cyclops_HUD.hornObject.activeSelf && (bool)cyclops_HUD.ReflectionGet("hudActive"));
            }
            return true;
        }

        public static void Postfix(CyclopsMotorModeButton __instance)
        {
            SubRoot cyclops = (SubRoot)__instance.ReflectionGet("subRoot");
            if (cyclops != null)
            {
                String guid = GuidHelper.GetGuid(cyclops.gameObject);
                Multiplayer.Logic.Cyclops.ChangeEngineMode(guid, __instance.motorModeIndex);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
