using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NitroxClient.Services;

/// <summary>
///     Provides a way for the game the wait for Nitrox before initializing.
/// </summary>
internal sealed class ContinueGameStartAfterHostReadyService(ILogger<ContinueGameStartAfterHostReadyService> logger) : IHostedLifecycleService
{
    private static readonly TaskCompletionSource<bool> hostWaitTcs = new();
    private readonly ILogger<ContinueGameStartAfterHostReadyService> logger = logger;

    public static async Task WaitForHostAsync() => await hostWaitTcs.Task;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Host started: continuing game startup...");
        hostWaitTcs.TrySetResult(true);
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
