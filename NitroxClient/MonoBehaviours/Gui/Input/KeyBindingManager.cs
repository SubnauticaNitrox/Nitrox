using System.Collections.Generic;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

namespace NitroxClient.MonoBehaviours.Gui.Input;

public static class KeyBindingManager
{
    public const int NITROX_BASE_ID = 1000;
    public static int CurrentBaseId = NITROX_BASE_ID;

    public static List<KeyBinding> KeyBindings =
    [
        new ChatKeyBindingAction(),
        new DiscordFocusBindingAction()
    ];
}
