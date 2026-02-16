namespace Nitrox.Model.Packets.Core;

public interface IPacketProcessContext;

public interface IPacketProcessContext<TSenderRef> : IPacketProcessContext
{
    /// <summary>
    ///     The sender of the packet.
    /// </summary>
    public TSenderRef Sender { get; set; }
}
