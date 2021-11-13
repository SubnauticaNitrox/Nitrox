using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public class GameInput_SetupDefaultKeyboardBindings_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => GameInput.SetupDefaultKeyboardBindings());

        public static void Postfix()
        {
            foreach (KeyBindingManager.KeyBinding keyBinding in KeyBindingManager.KeyboardBindings)
            {
                GameInput.SetBindingInternal(keyBinding.Device, keyBinding.Button, GameInput.BindingSet.Primary, keyBinding.Default.ToString());
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
