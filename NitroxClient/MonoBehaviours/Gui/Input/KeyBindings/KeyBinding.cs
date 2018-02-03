using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings
{
    public class KeyBinding
    {
        public GameInput.Button Button { get; }
        public GameInput.Device Device { get; }
        public string Label { get; }
        public DefaultKeyBinding DefaultKeyBinding { get; }
        public KeyBindingAction Action { get; }

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
