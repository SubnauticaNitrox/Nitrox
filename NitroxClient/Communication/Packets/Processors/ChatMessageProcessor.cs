using System;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.Settings;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class ChatMessageProcessor(PlayerManager remotePlayerManager, LocalPlayer localPlayer) : IClientPacketProcessor<ChatMessage>
{
    private readonly LocalPlayer localPlayer = localPlayer;
    private readonly PlayerChatManager playerChatManager = PlayerChatManager.Instance;
    private readonly PlayerManager remotePlayerManager = remotePlayerManager;

    private readonly Color32 serverMessageColor = new Color32(0x8c, 0x00, 0xFF, 0xFF);

    public Task Process(ClientProcessorContext context, ChatMessage message)
    {
        if (message.PlayerId != ChatMessage.SERVER_ID)
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
        if (localPlayer.PlayerId == message.PlayerId)
        {
            playerName = localPlayer.PlayerName;
            color = localPlayer.PlayerSettings.PlayerColor;
        }
        else
        {
            Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(message.PlayerId);
            if (!remotePlayer.HasValue)
            {
                string playerTableFormatted = string.Join("\n", remotePlayerManager.GetAll().Select(ply => $"Name: '{ply.PlayerName}', Id: {ply.PlayerId}"));
                Log.Error($"Tried to add chat message for remote player that could not be found with id '${message.PlayerId}' and message: '{message.Text}'.\nAll remote players right now:\n{playerTableFormatted}");
                throw new Exception($"Tried to add chat message for remote player that could not be found with id '${message.PlayerId}' and message: '{message.Text}'.\nAll remote players right now:\n{playerTableFormatted}");
            }

            playerName = remotePlayer.Value.PlayerName;
            color = remotePlayer.Value.PlayerSettings.PlayerColor;
        }

        playerChatManager.AddMessage(playerName, message.Text, color.ToUnity());
        if (NitroxPrefs.ChatAutoOpen.Value)
        {
            playerChatManager.ShowChat();
        }
    }

    private void LogServerMessage(ChatMessage message)
    {
        playerChatManager.AddMessage("Server", message.Text, serverMessageColor);
        if (NitroxPrefs.ChatAutoOpen.Value)
        {
            playerChatManager.ShowChat();
        }
    }
}
