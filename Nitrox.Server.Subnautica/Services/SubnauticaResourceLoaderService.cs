using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nitrox.Server.Subnautica.Models.Resources.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Pre-warms Subnautica resources from files on server startup.
/// </summary>
internal class SubnauticaResourceLoaderService(IEnumerable<IGameResource> resources, MemoryService memoryService, ILogger<SubnauticaResourceLoaderService> logger) : IHostedService
{
    private readonly ILogger<SubnauticaResourceLoaderService> logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Stopwatch totalStopWatch = Stopwatch.StartNew();
        logger.ZLogDebug($"Loading {resources.Count():@ResourceCount} resources {string.Join(", ", resources.Select(r => r.GetType().Name).OrderBy(n => n)):@TypeNames}...");
        await Parallel.ForEachAsync(resources, cancellationToken, async (resource, token) =>
        {
            string resourceName = resource.GetType().Name;

            Stopwatch stopwatch = Stopwatch.StartNew();
            await resource.LoadAsync(token);
            await resource.CleanupAsync();
            logger.ZLogDebug($"Resource {resourceName:@TypeName} loaded in {Math.Round(stopwatch.Elapsed.TotalSeconds, 3):@Seconds} seconds");
        });
        logger.ZLogDebug($"All resources loaded in {Math.Round(totalStopWatch.Elapsed.TotalSeconds, 3):@Seconds} seconds");

        memoryService.QueueCompact();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
