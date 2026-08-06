using System.Collections.Generic;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

namespace NitroxClient.MonoBehaviours.Gui.Input;

public static class KeyBindingManager
{
    public const int NITROX_BASE_ID = 1000;

    public static List<KeyBinding> KeyBindings =
    [
        new ChatKeyBindingAction(),
        new DiscordFocusBindingAction(),
        new VehicleHornKeyBindingAction()
    ];

    public static GameInput.Button GetButton<T>() where T : KeyBinding
    {
        int index = KeyBindings.FindIndex(binding => binding is T);
        if (index < 0)
        {
            throw new System.InvalidOperationException($"No key binding is registered for {typeof(T).Name}");
        }

        return (GameInput.Button)(NITROX_BASE_ID + index);
    }
}
