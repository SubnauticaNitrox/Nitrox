using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nitrox.Model.Core;
using UnityEngine;

namespace NitroxClient.Services;

internal sealed class StatusService(ILogger<StatusService> logger) : IHostedService
{
    private readonly ILogger<StatusService> logger = logger;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"[Nitrox] [INFO] Using {NitroxEnvironment.VersionInfo} built on {NitroxEnvironment.BuildDate:F}");
        logger.LogInformation($"[Nitrox] [INFO] Game version: {Application.version}");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
