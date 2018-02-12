using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class ChatMessageProcessor : ClientPacketProcessor<ChatMessage>
    {
        private readonly PlayerManager remotePlayerManager;
        private readonly PlayerChatManager chatManager;

        public ChatMessageProcessor(PlayerManager remotePlayerManager, PlayerChatManager chatManager)
        {
            this.remotePlayerManager = remotePlayerManager;
            this.chatManager = chatManager;
        }

        public override void Process(ChatMessage message)
        {
            Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(message.PlayerId);
            
            if (remotePlayer.IsPresent())
            {
                RemotePlayer remotePlayerInstance = remotePlayer.Get();
                ChatLogEntry chatLogEntry = new ChatLogEntry(remotePlayerInstance.PlayerName, message.Text, remotePlayerInstance.PlayerSettings.PlayerColor);
                chatManager.WriteChatLogEntry(chatLogEntry);
            }
        }
    }
}
