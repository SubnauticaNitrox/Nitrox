using System.Threading.Tasks;

namespace Nitrox.Model.Packets.Core;

public interface IPacketProcessor;

/// <summary>
///     A packet processor. The <see cref="Process"/> method is called when a connection sends data.
/// </summary>
public interface IPacketProcessor<in TContext, in TPacket> : IPacketProcessor
    where TContext : IPacketProcessContext
    where TPacket : Packet
{
    /// <summary>
    ///     Processes an incoming packet of type <see cref="TPacket" />.
    /// </summary>
    /// <param name="context">The context provided to this processor containing data about its sender.</param>
    /// <param name="packet">The incoming packet data that should be processed</param>
    Task Process(TContext context, TPacket packet);
}
