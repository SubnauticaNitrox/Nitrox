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
    public string OriginName => Player.Name;
    public SessionId OriginId { get; init; }
    public Perms Permissions { get; init; }

    /// <summary>
    ///     Gets the player which issued the command.
    /// </summary>
    public Player Player { get; init; }

    public PlayerToServerCommandContext(IPacketSender packetSender, Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        this.packetSender = packetSender;
        Player = player;
        OriginId = player.SessionId;
        Permissions = player.Permissions;
    }

    public async Task ReplyAsync<T>(T data) => await SendAsync(OriginId, data);

    public async ValueTask SendAsync<T>(SessionId sessionId, T data)
    {
        switch (data)
        {
            case Packet packet:
                await packetSender.SendPacket(packet, sessionId);
                break;
            case string message when !string.IsNullOrWhiteSpace(message):
                await packetSender.SendPacket(new ChatMessage(SessionId.SERVER_ID, message), sessionId);
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
                await packetSender.SendPacketToAll(packet);
                break;
            case string message when !string.IsNullOrWhiteSpace(message):
                await packetSender.SendPacketToAll(new ChatMessage(SessionId.SERVER_ID, message));
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
                await packetSender.SendPacketToOthers(packet, OriginId);
                break;
            case string message when !string.IsNullOrWhiteSpace(message):
                await packetSender.SendPacketToOthers(new ChatMessage(SessionId.SERVER_ID, message), OriginId);
                break;
            default:
                ICommandContext.ThrowNotSupportedData(data);
                break;
        }
    }
}
