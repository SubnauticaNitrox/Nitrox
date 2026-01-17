using System.Net;
using System.Net.NetworkInformation;

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

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(30));
        await WaitForAvailablePortAsync(options.ServerPort, ct: cts.Token);
    }

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task WaitForAvailablePortAsync(int port, CancellationToken ct = default)
    {
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
                    logger.ZLogTrace($"Port {options.ServerPort:@Port} UDP is available");
                    break;
                }

                if (first)
                {
                    first = false;
                    PrintPortWarn(logger, port);
                }
                else
                {
                    PrintPortWarn(logger, port);
                }

                await Task.Delay(3000, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }

        static void PrintPortWarn(ILogger logger, int port) =>
            logger.ZLogWarning($"Port {port:@Port} UDP is already in use. Please change the server port or close out any program that may be using it. Retrying...");
    }
}
