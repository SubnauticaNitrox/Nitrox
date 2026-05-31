using Nitrox.Model.Core;
using Nitrox.Model.Platforms.OS.Shared;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class RestartService(IPacketSender packetSender, ILogger<RestartService> logger) : IHostedLifecycleService
{
    private readonly ILogger<RestartService> logger = logger;
    private readonly IPacketSender packetSender = packetSender;

    public bool RestartOnStop
    {
        get => Interlocked.CompareExchange(ref field, false, false);
        set => Interlocked.CompareExchange(ref field, value, !value);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        if (RestartOnStop)
        {
            await packetSender.SendPacketToAllAsync(new ChatMessage(SessionId.SERVER_ID, "Server is restarting..."));
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
