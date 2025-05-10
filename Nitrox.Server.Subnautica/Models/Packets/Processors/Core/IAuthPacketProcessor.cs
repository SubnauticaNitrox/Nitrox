using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

internal interface IAuthPacketProcessor : IPacketProcessor
{
    Task Process(AuthProcessorContext context, Packet packet);
}

/// <summary>
///     A server packet processor that handles a packet type for authenticated connections.
/// </summary>
/// <typeparam name="TPacket">The packet type this processor can handle.</typeparam>
internal interface IAuthPacketProcessor<in TPacket> : IAuthPacketProcessor, IPacketProcessor<AuthProcessorContext, TPacket> where TPacket : Packet
{
    Task IAuthPacketProcessor.Process(AuthProcessorContext context, Packet packet) => Process(context, packet);
}
