using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.Settings;
using UnityEngine.InputSystem;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

public class ChatKeyBindingAction : KeyBinding
{
    public ChatKeyBindingAction() : base("Nitrox_Settings_Keybind_OpenChat", "y") { }

    public override void Execute(InputAction.CallbackContext _)
    {
        // If slider is at 0, chat is completely disabled
        if (NitroxPrefs.ChatVisibilityDuration.Value <= 0f)
        {
            return;
        }

        // If no other UWE input field is currently active then allow chat to open.
        if (FPSInputModule.current.lastGroup == null && Multiplayer.Joined)
        {
            PlayerChatManager.Instance.SelectChat();
        }
    }
}
