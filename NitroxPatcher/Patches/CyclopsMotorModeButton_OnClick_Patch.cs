using Harmony;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Client
{
    public class CyclopsMotorModeButton_OnClick_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsMotorModeButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnClick", BindingFlags.Public | BindingFlags.Instance);

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
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
