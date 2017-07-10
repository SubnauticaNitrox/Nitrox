using System;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class Chat : MonoBehaviour
    {
        public void Awake()
        {
            DevConsole.RegisterConsoleCommand(this, "chat", true);
        }

        public void OnConsoleCommand_chat(NotificationCenter.Notification n)
        {
            if (n != null && n.data != null && n.data.Count > 0)
            {
                String text = "";

                for(int i = 0; i < n.data.Count; i++)
                {
                    String word = n.data[i].ToString();
                    text += word + " ";
                }

                Multiplayer.Logic.Chat.SendChatMessage(text);
                ErrorMessage.AddMessage("Me: " + text); // Assumption: This method will only be called in-game
            }
        }
    }
}
