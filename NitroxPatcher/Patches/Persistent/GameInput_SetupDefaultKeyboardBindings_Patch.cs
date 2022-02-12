using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public class GameInput_SetupDefaultKeyboardBindings_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => GameInput.SetupDefaultKeyboardBindings());

        public static void Postfix()
        {
            KeyBindingManager keyBindingManager = new();
            foreach (KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
            {
                GameInput.SetBindingInternal(keyBinding.Device, keyBinding.Button, keyBinding.DefaultKeyBinding.BindingSet, keyBinding.DefaultKeyBinding.Binding);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
