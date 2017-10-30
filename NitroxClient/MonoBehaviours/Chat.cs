using NitroxClient.GameLogic.ChatUI;
using NitroxClient.MonoBehaviours.Gui.Chat;
using System;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class Chat : MonoBehaviour
    {
        private PlayerChatManager chatManager = new PlayerChatManager();

        public void Awake()
        {
            DevConsole.RegisterConsoleCommand(this, "chat", true);
        }

        public void OnConsoleCommand_chat(NotificationCenter.Notification n)
        {
            if (n != null && n.data != null && n.data.Count > 0)
            {
                string text = "";

                for(int i = 0; i < n.data.Count; i++)
                {
                    string word = n.data[i].ToString();
                    text += word + " ";
                }

                Multiplayer.Logic.Chat.SendChatMessage(text);
                chatManager.WriteMessage("Me: " + text);
            }
        }
    }
}
