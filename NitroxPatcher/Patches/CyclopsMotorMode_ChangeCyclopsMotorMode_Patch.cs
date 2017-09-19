using Harmony;
using NitroxClient;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class CyclopsMotorMode_ChangeCyclopsMotorMode_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsMotorMode);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("ChangeCyclopsMotorMode", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(CyclopsMotorMode __instance, out int __state)
        {
            __state = (int)__instance.cyclopsMotorMode;
            return true;
        }

        public static void Postfix(CyclopsMotorMode __instance, CyclopsMotorMode.CyclopsMotorModes newMode, int __state)
        {
            if (__state != (int) newMode)
            {
                String guid = GuidHelper.GetGuid(__instance.subRoot.gameObject);
                Multiplayer.Logic.Cyclops.ChangeEngineMode(guid, (int)newMode);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
