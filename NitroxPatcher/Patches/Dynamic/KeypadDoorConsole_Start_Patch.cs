using System;
using System.Reflection;
using Harmony;

namespace NitroxPatcher.Patches.Dynamic
{
    public class KeypadDoorConsole_Start_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(KeypadDoorConsole);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(KeypadDoorConsole __instance)
        {
            if (!__instance.unlocked)
            {
                if (__instance.root)

                {
                    __instance.root.BroadcastMessage("UnlockDoor");
                }
                else

                {
                    __instance.BroadcastMessage("UnlockDoor");
                }
                __instance.UnlockDoor();
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
