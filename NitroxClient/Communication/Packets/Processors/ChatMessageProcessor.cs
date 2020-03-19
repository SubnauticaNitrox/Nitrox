using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class ChatMessageProcessor : ClientPacketProcessor<ChatMessage>
    {
        private readonly PlayerManager remotePlayerManager;
        private readonly PlayerChat playerChat;

        public ChatMessageProcessor(PlayerManager remotePlayerManager, PlayerChat playerChat)
        {
            this.remotePlayerManager = remotePlayerManager;
            this.playerChat = playerChat;
        }

        public override void Process(ChatMessage message)
        {
            if (message.PlayerId != ChatMessage.SERVER_ID)
            {
                LogClientMessage(message);
            }
            else
            {
                LogServerMessage(message);
            }
        }

        private void LogClientMessage(ChatMessage message)
        {
            Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(message.PlayerId);
            if (!remotePlayer.IsPresent())
            {
                return;
            }
            
            RemotePlayer remotePlayerInstance = remotePlayer.Get();
            playerChat.AddMessage(remotePlayerInstance.PlayerName, message.Text, remotePlayerInstance.PlayerSettings.PlayerColor);
            playerChat.ShowLog();
        }

        private void LogServerMessage(ChatMessage message)
        {
            playerChat.AddMessage("Server", message.Text, new UnityEngine.Color32(0x8c, 0x00, 0xFF, 0xFF));
            playerChat.ShowLog();
        }
    }
}
