using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxClient.GameLogic.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class CyclopsShieldButton_OnClick_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsShieldButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnClick", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsShieldButton __instance)
        {
            string guid = GuidHelper.GetGuid(__instance.subRoot.gameObject);
            Multiplayer.Logic.Cyclops.ActivateShield(guid);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
