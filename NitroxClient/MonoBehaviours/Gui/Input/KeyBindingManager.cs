using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;
using System;
using System.Collections.Generic;

namespace NitroxClient.MonoBehaviours.Gui.Input
{
    public class KeyBindingManager
    {
        public List<KeyBinding> keyboardKeyBindings { get; set; }

        public static int CHAT_KEY_VALUE = 45;
        public static string CHAT_KEY_LABEL = "Chat";

        public KeyBindingManager()
        {
            keyboardKeyBindings = new List<KeyBinding>();

            keyboardKeyBindings.Add(new KeyBinding(CHAT_KEY_VALUE, CHAT_KEY_LABEL, GameInput.Device.Keyboard, new DefaultKeyBinding("Y", GameInput.BindingSet.Primary), new ChatKeyBindingAction()));
        }

        // Returns highest custom key binding value. If no custom key bindings, returns 0. 
        public int GetHighestKeyBindingValue()
        {
            int value = 0;

            foreach(KeyBinding keyBinding in keyboardKeyBindings)
            {
                int keyValue = (int)keyBinding.button;
                if(keyValue > value)
                {
                    value = keyValue;
                }
            }

            return value;
        }

    }
}
