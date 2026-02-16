using Microsoft.Extensions.Logging.Abstractions;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

internal sealed record HostToServerCommandContext : ICommandContext
{
    private readonly IPacketSender packetSender;

    public ILogger Logger { get; set; } = NullLogger.Instance;
    public CommandOrigin Origin { get; init; } = CommandOrigin.SERVER;
    public string OriginName => "SERVER";
    public SessionId OriginId { get; init; } = 0;
    public Perms Permissions { get; init; } = Perms.HOST;

    public HostToServerCommandContext(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    public async Task ReplyAsync<T>(T data)
    {
        switch (data)
        {
            case Packet packet:
                await packetSender.SendPacketToAllAsync(packet);
                break;
            case string message when !string.IsNullOrWhiteSpace(message):
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }
                Logger.LogInformation(message);
                return;
            default:
                ICommandContext.ThrowNotSupportedData(data);
                break;
        }
    }

    /// <summary>
    ///     Sends packet to all connected players.
    /// </summary>
    /// <remarks>
    ///     Same as <see cref="SendToOthersAsync" /> because the origin is the server.
    /// </remarks>
    public async ValueTask SendToAllAsync<T>(T data) => await SendToOthersAsync(data);

    /// <summary>
    ///     Sends packet to all connected players.
    /// </summary>
    public async ValueTask SendToOthersAsync<T>(T data)
    {
        switch (data)
        {
            case Packet packet:
                await packetSender.SendPacketToOthersAsync(packet, OriginId);
                break;
            case string message when !string.IsNullOrWhiteSpace(message):
                await packetSender.SendPacketToOthersAsync(new ChatMessage(SessionId.SERVER_ID, message), OriginId);
                break;
            default:
                ICommandContext.ThrowNotSupportedData(data);
                break;
        }
    }

    public async ValueTask SendAsync<T>(SessionId sessionId, T data)
    {
        switch (data)
        {
            case Packet packet:
                await packetSender.SendPacketAsync(packet, sessionId);
                break;
            case string message when !string.IsNullOrWhiteSpace(message):
                await packetSender.SendPacketAsync(new ChatMessage(SessionId.SERVER_ID, message), sessionId);
                break;
            default:
                ICommandContext.ThrowNotSupportedData(data);
                break;
        }
    }
}
