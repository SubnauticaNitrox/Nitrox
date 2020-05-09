using System;
using System.Linq;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class ChatMessageProcessor : ClientPacketProcessor<ChatMessage>
    {
        private readonly PlayerManager remotePlayerManager;
        private readonly PlayerChatManager playerChatManager;

        private readonly Color32 serverMessageColor = new Color32(0x8c, 0x00, 0xFF, 0xFF);

        public ChatMessageProcessor(PlayerManager remotePlayerManager, PlayerChatManager playerChatManager)
        {
            this.remotePlayerManager = remotePlayerManager;
            this.playerChatManager = playerChatManager;
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
            if (!remotePlayer.HasValue)
            {
                string playerTableFormatted = string.Join("\n", remotePlayerManager.GetAll().Select(ply => $"Name: '{ply.PlayerName}', Id: {ply.PlayerId}"));
                Log.Error($"Tried to add chat message for remote player that could not be found with id '${message.PlayerId}' and message: '{message.Text}'.\nAll remote players right now:\n{playerTableFormatted}");
                throw new Exception($"Tried to add chat message for remote player that could not be found with id '${message.PlayerId}' and message: '{message.Text}'.\nAll remote players right now:\n{playerTableFormatted}");
            }

            RemotePlayer remotePlayerInstance = remotePlayer.Value;
            playerChatManager.AddMessage(remotePlayerInstance.PlayerName, message.Text, remotePlayerInstance.PlayerSettings.PlayerColor);
            playerChatManager.ShowChat();
        }

        private void LogServerMessage(ChatMessage message)
        {
            playerChatManager.AddMessage("Server", message.Text, serverMessageColor);
            playerChatManager.ShowChat();
        }
    }
}
