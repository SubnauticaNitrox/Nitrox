using NitroxClient.MonoBehaviours.Discord;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

public class DiscordFocusBindingAction : KeyBinding
{
    public DiscordFocusBindingAction() : base("Nitrox_Keybind_FocusDiscord", "f")
    {
    }

    public override void Execute(InputAction.CallbackContext _)
    {
        if (UnityEngine.Input.GetKey(KeyCode.LeftAlt))
        {
            DiscordJoinRequestGui.Select();
        }
    }
}
