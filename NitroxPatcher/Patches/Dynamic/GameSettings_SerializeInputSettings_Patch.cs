using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Dynamic
{
    public class GameSettings_SerializeInputSettings_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(GameSettings);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SerializeInputSettings", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Postfix(GameSettings.ISerializer serializer)
        {
            KeyBindingManager keyBindingManager = new KeyBindingManager();
            string serializerFormat = "Input/Binding/{0}/{1}/{2}";

            foreach (GameInput.BindingSet bindingSet in Enum.GetValues(typeof(GameInput.BindingSet)))
            {
                foreach (KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
                {
                    Log.Debug($"Getting keybinding: {keyBinding.Device}, {keyBinding.Label} ({keyBinding.Button}), {bindingSet}");
                    string binding = (string)ReflectionHelper.ReflectionCall<GameInput>(null, "GetBinding", new Type[] { typeof(GameInput.Device), typeof(GameInput.Button), typeof(GameInput.BindingSet) }, true, true, keyBinding.Device, keyBinding.Button, bindingSet);
                    string name = string.Format(serializerFormat, keyBinding.Device, keyBinding.Button, bindingSet);

                    ReflectionHelper.ReflectionCall<GameInput>(null, "SetBinding", new Type[] { typeof(GameInput.Device), typeof(GameInput.Button), typeof(GameInput.BindingSet), typeof(string) }, true, true, keyBinding.Device, keyBinding.Button, bindingSet, serializer.Serialize(name, binding));
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
