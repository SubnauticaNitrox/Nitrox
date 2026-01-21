using Nitrox.Model.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Core;

internal interface IAuthPacketProcessor : IPacketProcessor;

/// <summary>
///     A server packet processor that handles a packet type for authenticated connections.
/// </summary>
/// <typeparam name="TPacket">The packet type this processor can handle.</typeparam>
internal interface IAuthPacketProcessor<in TPacket> : IAuthPacketProcessor, IPacketProcessor<AuthProcessorContext, TPacket> where TPacket : Packet;
