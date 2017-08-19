using Harmony;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class CyclopsSilentRunningAbilityButton_StartCooldown_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsSilentRunningAbilityButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("StartCooldown", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsSilentRunningAbilityButton __instance)
        {
            String guid = GuidHelper.GetGuid(__instance.subRoot.gameObject);   
            Multiplayer.Logic.Cyclops.BeginSilentRunning(guid);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
