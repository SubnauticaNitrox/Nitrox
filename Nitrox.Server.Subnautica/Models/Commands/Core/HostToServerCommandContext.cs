using System;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

internal sealed record HostToServerCommandContext : ICommandContext
{
    private readonly IServerPacketSender packetSender;

    public ILogger Logger { get; set; }
    public CommandOrigin Origin { get; init; } = CommandOrigin.SERVER;
    public string OriginName => "SERVER";
    public SessionId OriginId { get; init; } = 0;
    public Perms Permissions { get; init; } = Perms.SUPERADMIN;

    public HostToServerCommandContext(IServerPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    public async Task MessageAsync(SessionId id, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        await SendAsync(new ChatMessage(OriginId, message), id);
    }

    public Task ReplyAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return Task.CompletedTask;
        }
        Logger.LogInformation(message);
        return Task.CompletedTask;
    }

    public async Task MessageAllAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        await packetSender.SendPacketToAll(new ChatMessage(OriginId, message));
        await ReplyAsync($"[BROADCAST] {message}");
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
}
