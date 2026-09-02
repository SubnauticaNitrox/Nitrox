using UnityEngine.InputSystem;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;

public abstract class KeyBinding(string buttonLabel, string defaultKeyboardKey, string defaultControllerKey = null)
{
    public string ButtonLabel { get; init; } = buttonLabel;

    /// <summary>
    ///     Default binding for the keyboard/mouse scheme. Either a bare key name (e.g. "p"), which is resolved
    ///     against the keyboard (i.e. "&lt;Keyboard&gt;/p"), or a full Input System control path
    ///     (e.g. "&lt;Mouse&gt;/middleButton") to bind to a different device.
    /// </summary>
    public string DefaultKeyboardKey { get; init; } = defaultKeyboardKey;
    public string DefaultControllerKey { get; init; } = defaultControllerKey;

    public abstract void Execute(InputAction.CallbackContext callbackContext);
}
