using System.Collections.Generic;
using System.Linq;
using Nitrox.Client.MonoBehaviours.Gui.Input.KeyBindings;
using Nitrox.Client.MonoBehaviours.Gui.Input.KeyBindings.Actions;

namespace Nitrox.Client.MonoBehaviours.Gui.Input
{
    public class KeyBindingManager
    {
        public List<KeyBinding> KeyboardKeyBindings { get; }

        private const int CHAT_KEY_VALUE = 45;
        private const string CHAT_KEY_LABEL = "Chat";

        public KeyBindingManager()
        {
            KeyboardKeyBindings = new List<KeyBinding>
            {
                // new bindings should not be set to a value equivalent to a pre-existing GameInput.Button enum or another custom binding
                new KeyBinding(CHAT_KEY_VALUE, CHAT_KEY_LABEL, GameInput.Device.Keyboard, new DefaultKeyBinding("Y", GameInput.BindingSet.Primary), new ChatKeyBindingAction())
            };
        }

        // Returns highest custom key binding value. If no custom key bindings, returns 0. 
        public int GetHighestKeyBindingValue()
        {
            return KeyboardKeyBindings.Select(keyBinding => (int)keyBinding.Button).DefaultIfEmpty(0).Max();
        }
    }
}
