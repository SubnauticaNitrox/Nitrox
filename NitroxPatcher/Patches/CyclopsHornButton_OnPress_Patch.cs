using Harmony;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class CyclopsHornButton_OnPress_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsHornButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPress", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsHornButton __instance)
        {
            String guid = GuidHelper.GetGuid(__instance.subRoot.gameObject);
            Multiplayer.Logic.Cyclops.ActivateHorn(guid);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
