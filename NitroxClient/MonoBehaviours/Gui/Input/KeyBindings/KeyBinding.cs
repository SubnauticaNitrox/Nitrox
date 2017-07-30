using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings
{
    public class KeyBinding
    {
        public GameInput.Button button { get; set; }
        public GameInput.Device device { get; set; }
        public string label { get; set; }
        public DefaultKeyBinding defaultKeyBinding { get; set; }
        public KeyBindingAction action { get; set; }

        public KeyBinding(int keyBindingValue, string buttonLabel, GameInput.Device buttonDevice, DefaultKeyBinding buttonDefaults, KeyBindingAction buttonAction)
        {
            button = (GameInput.Button)keyBindingValue;
            device = buttonDevice;
            label = buttonLabel;
            action = buttonAction;
            defaultKeyBinding = buttonDefaults;
        }
    }
}
