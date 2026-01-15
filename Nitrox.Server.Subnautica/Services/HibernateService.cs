using Nitrox.Server.Subnautica.Models.AppEvents;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class HibernateService(IHibernate.SleepTrigger sleepTrigger, IHibernate.WakeTrigger wakeTrigger, ILogger<HibernateService> logger) : IHostedLifecycleService
{
    private readonly IHibernate.SleepTrigger sleepTrigger = sleepTrigger;
    private readonly IHibernate.WakeTrigger wakeTrigger = wakeTrigger;
    private readonly ILogger<HibernateService> logger = logger;

    public bool IsSleeping
    {
        get => Interlocked.CompareExchange(ref field, true, true);
        private set => Interlocked.Exchange(ref field, value);
    }

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
        IsSleeping = true;
        await sleepTrigger.InvokeAsync();
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
        IsSleeping = false;
        await wakeTrigger.InvokeAsync();
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken) => await SleepAsync(); // Start in sleep mode (ensures sleep tasks are executed at least once)

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StoppingAsync(CancellationToken cancellationToken) => await SleepAsync();

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
