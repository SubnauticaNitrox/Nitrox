using System.Reflection;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public partial class GameInputSystem_PopulateBindingSettings_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((GameInputSystem t) => t.PopulateBindingSettings(default(uGUI_OptionsPanel), default(int), default(GameInput.Device)));

        public static void Postfix(uGUI_OptionsPanel panel, int tabIndex, GameInput.Device device)
        {
            KeyBindingManager keyBindingManager = new();
            if (device == GameInput.Device.Keyboard)
            {
                foreach (KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
                {
                    panel.AddBindingOption(tabIndex, keyBinding.Label, keyBinding.Device, keyBinding.Button);
                }
            }
        }
    }
}
