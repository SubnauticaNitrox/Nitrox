using System;
using System.Linq;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.Settings;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class ChatMessageProcessor : IClientPacketProcessor<ChatMessage>
    {
        private readonly PlayerManager remotePlayerManager;
        private readonly LocalPlayer localPlayer;
        private readonly PlayerChatManager playerChatManager;

        private readonly Color32 serverMessageColor = new Color32(0x8c, 0x00, 0xFF, 0xFF);

        public ChatMessageProcessor(PlayerManager remotePlayerManager, LocalPlayer localPlayer, PlayerChatManager playerChatManager)
        {
            this.remotePlayerManager = remotePlayerManager;
            this.localPlayer = localPlayer;
            this.playerChatManager = playerChatManager;
        }

        public Task Process(IPacketProcessContext context, ChatMessage message)
        {
            if (message.SenderId != PeerId.SERVER_ID)
            {
                LogClientMessage(message);
            }
            else
            {
                LogServerMessage(message);
            }
            return Task.CompletedTask;
        }

        private void LogClientMessage(ChatMessage message)
        {
            // The message can come from either the local player or other players
            string playerName;
            NitroxColor color;
            if (localPlayer.PlayerId == message.SenderId)
            {
                playerName = localPlayer.PlayerName;
                color = localPlayer.PlayerSettings.PlayerColor;
            }
            else
            {
                Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(message.SenderId);
                if (!remotePlayer.HasValue)
                {
                    string playerTableFormatted = string.Join("\n", remotePlayerManager.GetAll().Select(ply => $"Name: '{ply.PlayerName}', Id: {ply.PlayerId}"));
                    Log.Error($"Tried to add chat message for remote player that could not be found with id '${message.SenderId}' and message: '{message.Text}'.\nAll remote players right now:\n{playerTableFormatted}");
                    throw new Exception($"Tried to add chat message for remote player that could not be found with id '${message.SenderId}' and message: '{message.Text}'.\nAll remote players right now:\n{playerTableFormatted}");
                }

                playerName = remotePlayer.Value.PlayerName;
                color = remotePlayer.Value.PlayerSettings.PlayerColor;
            }
            
            playerChatManager.AddMessage(playerName, message.Text, color.ToUnity());
            if (!NitroxPrefs.SilenceChat.Value)
            {
                playerChatManager.ShowChat();
            }
        }

        private void LogServerMessage(ChatMessage message)
        {
            playerChatManager.AddMessage("Server", message.Text, serverMessageColor);
            if (!NitroxPrefs.SilenceChat.Value)
            {
                playerChatManager.ShowChat();
            }
        }
    }
}
