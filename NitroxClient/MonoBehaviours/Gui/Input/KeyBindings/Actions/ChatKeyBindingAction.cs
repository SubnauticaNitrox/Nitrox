using System;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Core;
using UnityEngine.InputSystem;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

public class ChatKeyBindingAction : KeyBinding
{
    private readonly Lazy<PlayerChatManager> playerChatManager = new(NitroxServiceLocator.LocateService<PlayerChatManager>);

    public ChatKeyBindingAction() : base("Nitrox_Keybind_OpenChat", "y")
    {
    }

    public override void Execute(InputAction.CallbackContext _)
    {
        // If no other UWE input field is currently active then allow chat to open.
        if (FPSInputModule.current.lastGroup == null)
        {
            playerChatManager.Value.SelectChat();
        }
    }
}
