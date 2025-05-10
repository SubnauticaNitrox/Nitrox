using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using NitroxModel.Helper;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which waits for the configured port to be available.
/// </summary>
internal sealed class NetworkPortAvailabilityService(IOptions<SubnauticaServerOptions> options, ILogger<NetworkPortAvailabilityService> logger) : IHostedLifecycleService
{
    private readonly ILogger<NetworkPortAvailabilityService> logger = logger;
    private readonly SubnauticaServerOptions options = options.Value;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken) => await WaitForAvailablePortAsync(options.ServerPort, ct: cancellationToken);

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task WaitForAvailablePortAsync(int port, TimeSpan timeout = default, CancellationToken ct = default)
    {
        if (timeout == default)
        {
            timeout = TimeSpan.FromSeconds(30);
        }
        else
        {
            Validate.IsTrue(timeout.TotalSeconds >= 5, "Timeout must be at least 5 seconds.");
        }

        DateTimeOffset time = DateTimeOffset.UtcNow;
        bool first = true;
        try
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                IPEndPoint endPoint = null;
                foreach (IPEndPoint ip in IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners())
                {
                    if (ip.Port == port)
                    {
                        endPoint = ip;
                        break;
                    }
                }
                if (endPoint == null)
                {
                    logger.ZLogDebug($"Port {options.ServerPort:@Port} UDP is available");
                    break;
                }

                if (first)
                {
                    first = false;
                    PrintPortWarn(logger, port, timeout);
                }
                else
                {
                    PrintPortWarn(logger, port, timeout - (DateTimeOffset.UtcNow - time));
                }

                await Task.Delay(3000, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }

        static void PrintPortWarn(ILogger logger, int port, TimeSpan timeRemaining) =>
            logger.LogWarning("Port {port} UDP is already in use. Please change the server port or close out any program that may be using it. Retrying for {Seconds} seconds until it is available...", port, Math.Floor(timeRemaining.TotalSeconds));
    }
}
