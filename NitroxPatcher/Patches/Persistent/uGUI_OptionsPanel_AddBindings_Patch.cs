using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public class uGUI_OptionsPanel_AddBindings_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_OptionsPanel t) => t.AddBindings(default(int), default(GameInput.Device)));

        public static void Postfix(uGUI_OptionsPanel __instance, int tabIndex, GameInput.Device device)
        {
            KeyBindingManager keyBindingManager = new();
            if (device == GameInput.Device.Keyboard)
            {
                foreach (KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
                {
                    __instance.AddBindingOption(tabIndex, keyBinding.Label, keyBinding.Device, keyBinding.Button);
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
