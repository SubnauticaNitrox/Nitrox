using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class GameSettings_SerializeInputSettings_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => GameSettings.SerializeInputSettings(default(GameSettings.ISerializer)));

        public static void Postfix(GameSettings.ISerializer serializer)
        {
            KeyBindingManager keyBindingManager = new();
            string serializerFormat = "Input/Binding/{0}/{1}/{2}";

            foreach (GameInput.BindingSet bindingSet in Enum.GetValues(typeof(GameInput.BindingSet)))
            {
                foreach (KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
                {
                    Log.Debug($"Getting keybinding: {keyBinding.Device}, {keyBinding.Label} ({keyBinding.Button}), {bindingSet}");
                    string binding = GameInput.GetBinding(keyBinding.Device, keyBinding.Button, bindingSet);
                    string name = string.Format(serializerFormat, keyBinding.Device, keyBinding.Button, bindingSet);

                    GameInput.SetBinding(keyBinding.Device, keyBinding.Button, bindingSet, serializer.Serialize(name, binding));
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
