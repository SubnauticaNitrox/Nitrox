using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Resources.Helper;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Pre-warms Subnautica resources from files on server startup.
/// </summary>
internal class SubnauticaResourceLoaderService(IEnumerable<IGameResource> resources, SubnauticaAssetsManager subnauticaAssets, ILogger<SubnauticaResourceLoaderService> logger) : IHostedService
{
    private readonly SubnauticaAssetsManager subnauticaAssets = subnauticaAssets;
    private readonly ILogger<SubnauticaResourceLoaderService> logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.ZLogDebug($"Loading {resources.Count():@ResourceCount} resources {string.Join(", ", resources.Select(r => r.GetType().Name).OrderBy(n => n)):@TypeNames}...", resources.Count());
        await Parallel.ForEachAsync(resources, cancellationToken, async (resource, token) =>
        {
            string resourceName = resource.GetType().Name;

            Stopwatch stopwatch = Stopwatch.StartNew();
            await resource.LoadAsync(token);
            logger.ZLogDebug($"Resource {resourceName:@TypeName} loaded in {Math.Round(stopwatch.Elapsed.TotalSeconds, 3):@Seconds} seconds");
        });
        subnauticaAssets.UnloadAll(true);
        logger.ZLogDebug($"All game resources are loaded");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
