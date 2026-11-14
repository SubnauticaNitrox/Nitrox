using Microsoft.Extensions.Logging.Abstractions;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

internal sealed record PlayerToServerCommandContext : ICommandContext
{
    private readonly IPacketSender packetSender;
    public ILogger Logger { get; set; } = NullLogger.Instance;
    public CommandOrigin Origin { get; init; } = CommandOrigin.PLAYER;
    public string OriginName { get; init; }
    public SessionId OriginId { get; init; }
    public Perms Permissions { get; init; }

    public PlayerToServerCommandContext(IPacketSender packetSender, SessionId sessionId, string name, Perms permissions)
    {
        this.packetSender = packetSender;
        OriginId = sessionId;
        OriginName = name;
        Permissions = permissions;
    }

    public async Task ReplyAsync<T>(T data) => await SendAsync(OriginId, data);

    public async ValueTask SendAsync<T>(SessionId sessionId, T data)
    {
        switch (data)
        {
            case Packet packet:
                await packetSender.SendPacketAsync(packet, sessionId);
                break;
            case string message:
                if (!string.IsNullOrWhiteSpace(message))
                {
                    await packetSender.SendPacketAsync(new ChatMessage((SessionId)SessionId.SERVER_ID, message), sessionId);
                }
                break;
            default:
                ICommandContext.ThrowNotSupportedData(data);
                break;
        }
    }

    public async ValueTask SendToAllAsync<T>(T data)
    {
        switch (data)
        {
            case Packet packet:
                await packetSender.SendPacketToAllAsync(packet);
                break;
            case string message:
                if (!string.IsNullOrWhiteSpace(message))
                {
                    await packetSender.SendPacketToAllAsync(new ChatMessage((SessionId)SessionId.SERVER_ID, message));
                }
                break;
            default:
                ICommandContext.ThrowNotSupportedData(data);
                break;
        }
    }

    public async ValueTask SendToOthersAsync<T>(T data)
    {
        switch (data)
        {
            case Packet packet:
                await packetSender.SendPacketToOthersAsync(packet, OriginId);
                break;
            case string message:
                if (!string.IsNullOrWhiteSpace(message))
                {
                    await packetSender.SendPacketToOthersAsync(new ChatMessage((SessionId)SessionId.SERVER_ID, message), OriginId);
                }
                break;
            default:
                ICommandContext.ThrowNotSupportedData(data);
                break;
        }
    }

    public override string ToString() => $"'{OriginName}' #{OriginId}";
}
