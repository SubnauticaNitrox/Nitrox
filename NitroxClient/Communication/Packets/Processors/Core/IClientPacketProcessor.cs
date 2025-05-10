using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors.Core;

public interface IClientPacketProcessor;

public interface IClientPacketProcessor<in TPacket> : IClientPacketProcessor, IPacketProcessor<IPacketProcessContext, TPacket> where TPacket : Packet;
