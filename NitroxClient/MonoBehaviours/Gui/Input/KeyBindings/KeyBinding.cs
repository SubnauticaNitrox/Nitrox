using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;
using NitroxModel.Helper;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;

public class KeyBinding
{
    public GameInput.Button Button { get; }
    public GameInput.Device Device { get; }
    public string Label { get; }
    public string PrimaryKey { get; }
    public string SecondaryKey { get; }
    public KeyBindingAction Action { get; }

    public KeyBinding(int keyBindingValue, string buttonLabel, GameInput.Device buttonDevice, KeyBindingAction buttonAction, string primaryKey, string secondaryKey = null)
    {
        Validate.NotNull(primaryKey);

        Button = (GameInput.Button)keyBindingValue;
        Device = buttonDevice;
        Label = buttonLabel;
        Action = buttonAction;
        PrimaryKey = primaryKey;
        SecondaryKey = secondaryKey;
    }
}
