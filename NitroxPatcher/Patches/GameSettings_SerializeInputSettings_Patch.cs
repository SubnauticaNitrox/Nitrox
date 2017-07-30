using Harmony;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxModel.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class GameSettings_SerializeInputSettings_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(GameSettings);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SerializeInputSettings", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Postfix(GameSettings.ISerializer serializer)
        {
            KeyBindingManager keyBindingManager = new KeyBindingManager();
            string serializerFormat = "Input/Binding/{0}/{1}/{2}";

            foreach(GameInput.BindingSet bindingSet in Enum.GetValues(typeof(GameInput.BindingSet)))
            {
                foreach (KeyBinding keyBinding in keyBindingManager.keyboardKeyBindings)
                {
                    string binding = (string)ReflectionHelper.ReflectionCall(typeof(GameInput), "GetBinding", new Type[] { typeof(GameInput.Device), typeof(GameInput.Button), typeof(GameInput.BindingSet) }, true, true, keyBinding.device, keyBinding.button, bindingSet);
                    string name = string.Format(serializerFormat, keyBinding.device, keyBinding.button, bindingSet);

                    ReflectionHelper.ReflectionCall(typeof(GameInput), "SetBinding", new Type[] { typeof(GameInput.Device), typeof(GameInput.Button), typeof(GameInput.BindingSet), typeof(string) }, true, true, keyBinding.device, keyBinding.button, bindingSet, serializer.Serialize(name, binding));
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
