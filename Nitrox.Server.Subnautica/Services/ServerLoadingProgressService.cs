using System.Collections.Generic;
using System.Threading.Channels;
using Nitrox.Server.Subnautica.Services.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Tracks server loading progress and provides updates to connected management clients (e.g., Nitrox Launcher).
/// </summary>
internal sealed class ServerLoadingProgressService : IHostedLifecycleService, IProgressReporter
{
    private readonly Channel<IProgressReporter.ProgressUpdate> progressQueue = Channel.CreateBounded<IProgressReporter.ProgressUpdate>(new BoundedChannelOptions(16) { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true });

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        await ReportProgressAsync("Initializing server", 0.0f);
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        // Clear loading state - server is now fully ready
        await ReportProgressAsync("", 1.0f);
        progressQueue.Writer.TryComplete();
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task ReportProgressAsync(string stage, float progress)
    {
        await progressQueue.Writer.WriteAsync(new IProgressReporter.ProgressUpdate(stage, Math.Clamp(progress, 0f, 1f)));
    }

    public IAsyncEnumerable<IProgressReporter.ProgressUpdate> ReadAllAsync(CancellationToken cancellationToken) => progressQueue.Reader.ReadAllAsync(cancellationToken);
}
