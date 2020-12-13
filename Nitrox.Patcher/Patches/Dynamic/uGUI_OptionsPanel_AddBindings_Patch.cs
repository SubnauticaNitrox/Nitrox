using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.MonoBehaviours.Gui.Input;
using Nitrox.Client.MonoBehaviours.Gui.Input.KeyBindings;
using Nitrox.Model.Helper;

namespace Nitrox.Patcher.Patches.Dynamic
{
    public class uGUI_OptionsPanel_AddBindings_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(uGUI_OptionsPanel);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("AddBindings", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(uGUI_OptionsPanel __instance, int tabIndex, GameInput.Device device)
        {
            KeyBindingManager keyBindingManager = new KeyBindingManager();

            if (device == GameInput.Device.Keyboard)
            {
                foreach (KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
                {
                    ReflectionHelper.ReflectionCall(__instance, "AddBindingOption", new Type[] { typeof(int), typeof(string), typeof(GameInput.Device), typeof(GameInput.Button) }, true, false, tabIndex, keyBinding.Label, keyBinding.Device, keyBinding.Button);
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
