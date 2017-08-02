using Harmony;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxModel.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class GameInput_SetupDefaultKeyboardBindings_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(GameInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetupDefaultKeyboardBindings", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Postfix()
        {
            KeyBindingManager keyBindingManager = new KeyBindingManager();
            
            foreach(KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
            {
                ReflectionHelper.ReflectionCall(typeof(GameInput), "SetBindingInternal", new Type[] { typeof(GameInput.Device), typeof(GameInput.Button), typeof(GameInput.BindingSet), typeof(string) }, false, true, keyBinding.Device, keyBinding.Button, keyBinding.DefaultKeyBinding.BindingSet, keyBinding.DefaultKeyBinding.Binding);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
