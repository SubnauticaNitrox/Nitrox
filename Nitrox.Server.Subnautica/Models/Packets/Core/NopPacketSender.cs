using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Core;

internal sealed class NopPacketSender : IPacketSender
{
    public ValueTask SendPacketAsync<T>(T packet, SessionId sessionId) where T : Packet => ValueTask.CompletedTask;

    public ValueTask SendPacketToAllAsync<T>(T packet) where T : Packet => ValueTask.CompletedTask;

    public ValueTask SendPacketToOthersAsync<T>(T packet, SessionId excludedSessionId) where T : Packet => ValueTask.CompletedTask;
}
