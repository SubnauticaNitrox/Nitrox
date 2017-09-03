using NitroxClient.GameLogic.ChatUI;
using System;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions
{
    public class ChatKeyBindingAction : KeyBindingAction
    {
        PlayerChatManager chatManager = new PlayerChatManager();

        public override void Execute()
        {
            chatManager.ShowChat();
        }
    }
}
