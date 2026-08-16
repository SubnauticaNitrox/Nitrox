using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Model.Packets;

namespace NitroxClient.Services;

internal sealed class PacketSerializationService : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Packet.InitSerializer();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
