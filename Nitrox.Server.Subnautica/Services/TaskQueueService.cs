using System.Threading.Channels;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Tasks can be queued here to be awaited before the server stops. Any critical work will then have a chance to finish processing or throw errors.
/// </summary>
internal sealed class TaskQueueService : BackgroundService, IHostedLifecycleService
{
    private readonly Channel<Task> taskChannel = Channel.CreateUnbounded<Task>(new UnboundedChannelOptions
    {
        SingleReader = true
    });

    public bool TryQueue(Task task) => taskChannel.Writer.TryWrite(task);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (Task task in taskChannel.Reader.ReadAllAsync(stoppingToken))
        {
            await task;
        }
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StoppedAsync(CancellationToken cancellationToken)
    {
        taskChannel.Writer.TryComplete();
        // Finish processing of remaining tasks.
        await foreach (Task task in taskChannel.Reader.ReadAllAsync(cancellationToken))
        {
            await task;
        }
    }
}
