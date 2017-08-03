using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings
{
    public class KeyBinding
    {
        public GameInput.Button Button { get; private set; }
        public GameInput.Device Device { get; private set; }
        public string Label { get; private set; }
        public DefaultKeyBinding DefaultKeyBinding { get; private set; }
        public KeyBindingAction Action { get; private set; }

        public KeyBinding(int keyBindingValue, string buttonLabel, GameInput.Device buttonDevice, DefaultKeyBinding buttonDefaults, KeyBindingAction buttonAction)
        {
            Button = (GameInput.Button)keyBindingValue;
            Device = buttonDevice;
            Label = buttonLabel;
            Action = buttonAction;
            DefaultKeyBinding = buttonDefaults;
        }
    }
}
