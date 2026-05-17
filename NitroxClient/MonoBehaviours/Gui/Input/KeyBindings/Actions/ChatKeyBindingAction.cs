using NitroxClient.GameLogic.ChatUI;
using UnityEngine.InputSystem;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

public class ChatKeyBindingAction : KeyBinding
{
    public ChatKeyBindingAction() : base("Nitrox_Settings_Keybind_OpenChat", "y", "dpad/up") { }

    public override void Execute(InputAction.CallbackContext _)
    {
        // If no other UWE input field is currently active then allow chat to open.
        if (FPSInputModule.current.lastGroup == null && Multiplayer.Joined)
        {
            PlayerChatManager.Instance.SelectChat();
        }
    }
}
