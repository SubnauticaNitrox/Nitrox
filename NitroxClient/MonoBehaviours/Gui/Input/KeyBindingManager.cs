using System.Collections.Generic;
using System.Linq;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

namespace NitroxClient.MonoBehaviours.Gui.Input
{
    public class KeyBindingManager
    {
        public List<KeyBinding> KeyboardKeyBindings { get; }

        public KeyBindingManager()
        {
            KeyboardKeyBindings = new List<KeyBinding>
            {
                // new bindings should not be set to a value equivalent to a pre-existing GameInput.Button enum or another custom binding
                new(45, "Chat", GameInput.Device.Keyboard, new DefaultKeyBinding("Y", GameInput.BindingSet.Primary), new ChatKeyBindingAction()),
                new(46, "Focus Discord (Alt +)", GameInput.Device.Keyboard, new DefaultKeyBinding("F", GameInput.BindingSet.Primary), new DiscordFocusBindingAction())
            };
        }

        // Returns highest custom key binding value. If no custom key bindings, returns 0. 
        public int GetHighestKeyBindingValue()
        {
            return KeyboardKeyBindings.Select(keyBinding => (int)keyBinding.Button).DefaultIfEmpty(0).Max();
        }
    }
}
