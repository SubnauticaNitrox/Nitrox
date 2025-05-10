using System;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

internal sealed record PlayerToServerCommandContext : ICommandContext
{
    private readonly IServerPacketSender packetSender;
    public ILogger Logger { get; set; }
    public CommandOrigin Origin { get; init; } = CommandOrigin.PLAYER;
    public string OriginName => Player.Name;
    public SessionId OriginId { get; init; }
    public Perms Permissions { get; init; }

    /// <summary>
    ///     Gets the player which issued the command.
    /// </summary>
    public ConnectedPlayerDto Player { get; init; }

    public PlayerToServerCommandContext(IServerPacketSender packetSender, ConnectedPlayerDto player)
    {
        ArgumentNullException.ThrowIfNull(player);
        this.packetSender = packetSender;
        Player = player;
        OriginId = player.SessionId;
        Permissions = player.Permissions;
    }

    public async Task MessageAsync(SessionId id, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        if (OriginId == id)
        {
            await ReplyAsync(message);
            return;
        }
        await packetSender.SendPacket(new ChatMessage(OriginId, message), id);
    }

    public async Task ReplyAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        await packetSender.SendPacket(new ChatMessage(SessionId.SERVER_ID, message), OriginId);
    }

    public async Task MessageAllAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        await packetSender.SendPacketToOthers(new ChatMessage(SessionId.SERVER_ID, message), OriginId);
        Logger.ZLogInformation($"Player {Player.Name:@PlayerName} #{Player.Id:@PlayerId} sent a message to everyone:{message:@ChatMessage}");
    }

    public async ValueTask SendAsync<T>(T data, SessionId sessionId)
    {
        switch (data)
        {
            case Packet packet:
                await packetSender.SendPacket(packet, sessionId);
                break;
            default:
                throw new NotSupportedException($"Unsupported data type {data?.GetType()}");
        }
    }

    public async ValueTask SendToAll<T>(T data)
    {
        switch (data)
        {
            case Packet packet:
                await packetSender.SendPacketToAll(packet);
                break;
            default:
                throw new NotSupportedException($"Unsupported data type {data?.GetType()}");
        }
    }
}
