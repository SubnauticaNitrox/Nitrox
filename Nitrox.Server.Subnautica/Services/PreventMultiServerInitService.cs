namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Waits for other Nitrox servers to finish starting, if any.
/// </summary>
internal sealed class PreventMultiServerInitService(ILogger<PreventMultiServerInitService> logger) : IHostedLifecycleService
{
    private readonly SemaphoreSlim callerGate = new(1);
    private readonly ILogger<PreventMultiServerInitService> logger = logger;
    private readonly SemaphoreSlim mutexReleaseGate = new(1);

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        ReleaseMutex();
        return Task.CompletedTask;
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        logger.ZLogDebug($"Taking mutex lock on server initializations");
        await HoldMutexAsync(cancellationToken);
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        ReleaseMutex();
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task HoldMutexAsync(CancellationToken ct = default)
    {
        Thread thread = new(o =>
        {
            bool first = true;
            Mutex mutex = new(false, typeof(PreventMultiServerInitService).Assembly.FullName, out bool _);
            try
            {
                try
                {
                    while (!mutex.WaitOne(100, false))
                    {
                        ct.ThrowIfCancellationRequested();
                        if (first)
                        {
                            first = false;
                        }
                    }
                }
                catch (AbandonedMutexException)
                {
                    // Mutex was abandoned in another process, it will still get acquired
                }
            }
            catch (OperationCanceledException)
            {
                logger.ZLogDebug($"Holding mutex was cancelled");
            }
            finally
            {
                callerGate.Release();
                mutexReleaseGate.Wait(-1);
                mutex.ReleaseMutex();
                logger.ZLogDebug($"Releasing hold on server initializations");
            }
        });
        await mutexReleaseGate.WaitAsync(-1, ct);
        await callerGate.WaitAsync(0, ct);
        thread.Start();

        while (!await callerGate.WaitAsync(100, ct))
        {
        }
    }

    private void ReleaseMutex()
    {
        mutexReleaseGate.Release();
    }
}
