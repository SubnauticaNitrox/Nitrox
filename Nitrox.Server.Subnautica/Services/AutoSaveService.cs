namespace Nitrox.Server.Subnautica.Services;

internal sealed class AutoSaveService(SaveService saveService, HibernateService hibernateService, IOptions<SubnauticaServerOptions> options, ILogger<AutoSaveService> logger) : BackgroundService, IHostedLifecycleService
{
    private readonly HibernateService hibernateService = hibernateService;
    private readonly ILogger<AutoSaveService> logger = logger;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly SaveService saveService = saveService;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        logger.ZLogTrace($"{(options.Value.ShouldAutoSave() ? $"ENABLED ({TimeSpan.FromMilliseconds(options.Value.SaveInterval).TotalMinutes} min)" : "DISABLED")}");
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.ShouldAutoSave())
        {
            logger.ZLogTrace($"DISABLED");
            return;
        }

        using PeriodicTimer autoSaveTimer = new(TimeSpan.FromMilliseconds(options.Value.SaveInterval));
        while (!stoppingToken.IsCancellationRequested)
        {
            await autoSaveTimer.WaitForNextTickAsync(stoppingToken);
            if (!options.Value.ShouldAutoSave())
            {
                continue;
            }
            if (hibernateService.IsSleeping)
            {
                logger.ZLogTrace($"Auto saving skipped because server is in power saving mode");
                continue;
            }

            logger.ZLogTrace($"Requesting to save...");
            await saveService.QueueActionAsync(SaveService.ServiceAction.SAVE, stoppingToken);
        }
    }
}
