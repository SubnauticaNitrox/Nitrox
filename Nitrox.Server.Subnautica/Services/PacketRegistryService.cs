extern alias JB;
using Nitrox.Model.Packets.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Collects packet processors into a fast lookup, based on the packet type they can handle.
/// </summary>
internal sealed class PacketRegistryService(Func<IPacketProcessor[]> packetProcessorsProvider) : IHostedService
{
    private PacketProcessorsInvoker packetProcessorsInvoker;
    private readonly Func<IPacketProcessor[]> packetProcessorsProvider = packetProcessorsProvider;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        packetProcessorsInvoker = new PacketProcessorsInvoker(packetProcessorsProvider());
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public PacketProcessorsInvoker.Entry? GetProcessor(Type packetType) => packetProcessorsInvoker.GetProcessor(packetType);
}
