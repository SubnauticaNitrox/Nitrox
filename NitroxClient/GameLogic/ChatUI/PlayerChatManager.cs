using NitroxClient.MonoBehaviours.Gui.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.GameLogic.ChatUI
{
    class PlayerChatManager
    {
        private PlayerChat chatEntry;
        
        public void WriteMessage(String message)
        {
            if(chatEntry == null)
            {
                chatEntry = new GameObject().AddComponent<PlayerChat>();
            }

            chatEntry.WriteMessage(message);
        }
    }
}
