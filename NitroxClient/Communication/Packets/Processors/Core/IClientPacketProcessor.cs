using Nitrox.Model.Packets;
using Nitrox.Model.Packets.Core;

namespace NitroxClient.Communication.Packets.Processors.Core;

public interface IClientPacketProcessor : IPacketProcessor;

public interface IClientPacketProcessor<in TPacket> : IClientPacketProcessor, IPacketProcessor<ClientProcessorContext, TPacket> where TPacket : Packet;
