using System.Reflection;
using Harmony;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public class GameInput_SetupDefaultKeyboardBindings_Patch : NitroxPatch, IPersistentPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(GameInput).GetMethod("SetupDefaultKeyboardBindings", BindingFlags.NonPublic | BindingFlags.Static);

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
