using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;
using System;
using System.Collections.Generic;

namespace NitroxClient.MonoBehaviours.Gui.Input
{
    public class KeyBindingManager
    {
        public List<KeyBinding> KeyboardKeyBindings { get; set; }

        public static int CHAT_KEY_VALUE = 45; 
        public static string CHAT_KEY_LABEL = "Chat";

        public KeyBindingManager()
        {
            KeyboardKeyBindings = new List<KeyBinding>();

            // new bindings should not be set to a value equivalent to a pre-existing GameInput.Button enum or another custom binding
            KeyboardKeyBindings.Add(new KeyBinding(CHAT_KEY_VALUE, CHAT_KEY_LABEL, GameInput.Device.Keyboard, new DefaultKeyBinding("Y", GameInput.BindingSet.Primary), new ChatKeyBindingAction()));
        }

        // Returns highest custom key binding value. If no custom key bindings, returns 0. 
        public int GetHighestKeyBindingValue()
        {
            int value = 0;

            foreach(KeyBinding keyBinding in KeyboardKeyBindings)
            {
                int keyValue = (int)keyBinding.Button;
                if(keyValue > value)
                {
                    value = keyValue;
                }
            }

            return value;
        }

    }
}
