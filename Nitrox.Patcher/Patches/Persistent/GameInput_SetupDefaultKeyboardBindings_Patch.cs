using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.MonoBehaviours.Gui.Input;
using Nitrox.Client.MonoBehaviours.Gui.Input.KeyBindings;
using Nitrox.Model.Helper;

namespace Nitrox.Patcher.Patches.Persistent
{
    public class GameInput_SetupDefaultKeyboardBindings_Patch : NitroxPatch, IPersistentPatch
    {
        public static readonly Type TARGET_CLASS = typeof(GameInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetupDefaultKeyboardBindings", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Postfix()
        {
            KeyBindingManager keyBindingManager = new KeyBindingManager();
            foreach (KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
            {
                ReflectionHelper.ReflectionCall<GameInput>(null, "SetBindingInternal", new[] { typeof(GameInput.Device), typeof(GameInput.Button), typeof(GameInput.BindingSet), typeof(string) }, false, true, keyBinding.Device, keyBinding.Button, keyBinding.DefaultKeyBinding.BindingSet, keyBinding.DefaultKeyBinding.Binding);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
