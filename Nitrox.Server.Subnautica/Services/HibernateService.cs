using Nitrox.Server.Subnautica.Models.Events;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class HibernateService(Func<IHibernate[]> hibernatorsProvider, ILogger<HibernateService> logger) : IHostedLifecycleService
{
    private readonly Func<IHibernate[]> hibernatorsProvider = hibernatorsProvider;
    private readonly ILogger<HibernateService> logger = logger;
    private bool isSleeping;
    public bool IsSleeping => Interlocked.CompareExchange(ref isSleeping, true, true);

    /// <summary>
    ///     Puts server in power saving mode. Should still allow server to wake up as if it never slept.
    /// </summary>
    public async Task SleepAsync()
    {
        if (IsSleeping)
        {
            return;
        }
        logger.ZLogInformation($"Entering power saving mode...");
        Interlocked.Exchange(ref isSleeping, true);
        foreach (IHibernate hibernator in hibernatorsProvider())
        {
            await hibernator.SleepAsync();
        }
    }

    /// <summary>
    ///     Wakes up the server which will enable and simulate all features.
    /// </summary>
    public async Task WakeAsync()
    {
        if (!IsSleeping)
        {
            return;
        }
        logger.ZLogInformation($"Entering full operation mode...");
        Interlocked.Exchange(ref isSleeping, false);
        foreach (IHibernate hibernator in hibernatorsProvider())
        {
            await hibernator.WakeAsync();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken) => await SleepAsync(); // Start in sleep mode (ensures sleep tasks are executed at least once)

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StoppingAsync(CancellationToken cancellationToken) => await SleepAsync();

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
