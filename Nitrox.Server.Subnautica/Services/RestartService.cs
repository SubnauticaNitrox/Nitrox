using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using NitroxModel.Networking.Packets;
using NitroxModel.Platforms.OS.Shared;

namespace Nitrox.Server.Subnautica.Services;

internal class RestartService(ILogger<RestartService> logger, IServerPacketSender packetSender) : IHostedLifecycleService
{
    private bool restartOnStop;
    private readonly ILogger<RestartService> logger = logger;
    private readonly IServerPacketSender packetSender = packetSender;

    public bool RestartOnStop
    {
        get => Interlocked.CompareExchange(ref restartOnStop, false, false);
        set => Interlocked.CompareExchange(ref restartOnStop, value, !value);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        if (RestartOnStop)
        {
            await packetSender.SendPacketToAll(new ChatMessage(SessionId.SERVER_ID, "Server is restarting..."));
        }
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        if (!RestartOnStop)
        {
            return Task.CompletedTask;
        }
        logger.ZLogInformation($"Server is restarting...");
        ProcessEx.StartSelfCopyArgs();
        return Task.CompletedTask;
    }
}
