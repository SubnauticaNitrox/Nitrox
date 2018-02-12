using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class Chat : MonoBehaviour
    {
        private GameLogic.Chat chatPacketSender;
        private PlayerChatManager chatManager;
        private PlayerManager remotePlayerManager;
        public static Chat Main { get; set; }

        public void Awake()
        {
            chatPacketSender = NitroxServiceLocator.LocateService<GameLogic.Chat>();
            chatManager = NitroxServiceLocator.LocateService<PlayerChatManager>();
            remotePlayerManager = NitroxServiceLocator.LocateService<PlayerManager>();

            DevConsole.RegisterConsoleCommand(this, "chat", true);
            Main = this;
        }

        public void OnConsoleCommand_chat(NotificationCenter.Notification n)
        {
            if (n != null && n.data != null && n.data.Count > 0)
            {
                string text = "";

                for (int i = 0; i < n.data.Count; i++)
                {
                    string word = n.data[i].ToString();
                    text += word + " ";
                }

                chatPacketSender.SendChatMessage(text);
                chatManager.WriteMessage("Me: " + text);
            }
        }

        public void WriteLocalMessage(ChatMessage message)
        {
            Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(message.PlayerId);

            //TODO: Figure out how to use color on PlayerSettings to set chat color
            if (remotePlayer.IsPresent())
            {
                chatManager.WriteMessage(remotePlayer.Get().PlayerName + ": " + message.Text);
            }
        }
    }
}
