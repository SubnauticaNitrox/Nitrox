using UnityEngine.InputSystem;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;

public abstract class KeyBinding(string buttonLabel, string defaultKeyboardKey, string defaultControllerKey = null)
{
    public string ButtonLabel { get; init; } = buttonLabel;
    public string DefaultKeyboardKey { get; init; } = defaultKeyboardKey;
    public string DefaultControllerKey { get; init; } = defaultControllerKey;

    public abstract void Execute(InputAction.CallbackContext callbackContext);
}
