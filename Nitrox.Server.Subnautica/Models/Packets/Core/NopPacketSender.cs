using Nitrox.Model.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Core;

internal sealed class NopPacketSender : IPacketSender
{
    public ValueTask SendPacket<T>(T packet, SessionId sessionId) where T : Packet => ValueTask.CompletedTask;

    public ValueTask SendPacketToAll<T>(T packet) where T : Packet => ValueTask.CompletedTask;

    public ValueTask SendPacketToOthers<T>(T packet, SessionId excludedSessionId) where T : Packet => ValueTask.CompletedTask;
}
