using Harmony;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxModel.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class GameInput_Initialize_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(GameInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(GameInput __instance)
        {
            KeyBindingManager keyBindingManager = new KeyBindingManager();

            int numDevices = (int)ReflectionHelper.ReflectionGet(__instance, "numDevices", false, true);
            int numButtons = keyBindingManager.GetHighestKeyBindingValue() + 1; // need enough space to support custom bindings
            int numBindingSets = (int)ReflectionHelper.ReflectionGet(__instance, "numBindingSets", false, true);

            ReflectionHelper.ReflectionSet(__instance, "numButtons", numButtons, false, true);
            ReflectionHelper.ReflectionSet(__instance, "buttonBindings", new Array3<int>(numDevices, numButtons, numBindingSets), false, true);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
