using NitroxClient.GameLogic.ChatUI;
using System;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions
{
    public class ChatKeyBindingAction : KeyBindingAction
    {
        public override void Execute()
        {
            PlayerChatManager chatManager = new PlayerChatManager();
            chatManager.ShowChat();
        }
    }
}
