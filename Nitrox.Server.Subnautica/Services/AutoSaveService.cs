namespace Nitrox.Server.Subnautica.Services;

internal sealed class AutoSaveService(SaveService saveService, Hibernator hibernator, IOptions<SubnauticaServerOptions> options, ILogger<AutoSaveService> logger) : BackgroundService
{
    private readonly SaveService saveService = saveService;
    private readonly Hibernator hibernator = hibernator;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ILogger<AutoSaveService> logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int saveInterval = options.Value.SaveInterval;
        if (saveInterval < 1)
        {
            logger.ZLogTrace($"disabled");
            return;
        }

        PeriodicTimer timer = new(TimeSpan.FromMilliseconds(saveInterval));
        while (!stoppingToken.IsCancellationRequested)
        {
            await timer.WaitForNextTickAsync(stoppingToken);
            if (options.Value is not { AutoSave: true, SaveInterval: > 0 })
            {
                continue;
            }
            if (hibernator.IsSleeping)
            {
                continue;
            }

            logger.ZLogTrace($"Requesting to save...");
            saveService.Save();
        }
    }
}
