using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which waits for the configured port to be available.
/// </summary>
internal sealed class NetworkPortAvailabilityService(IOptions<SubnauticaServerOptions> options, ILogger<NetworkPortAvailabilityService> logger) : IHostedLifecycleService
{
    private const int TIMEOUT_SECONDS = 30;
    private const int ITERATION_DELAY_MS = 3000;
    private readonly Stopwatch time = new();
    private readonly ILogger<NetworkPortAvailabilityService> logger = logger;
    private readonly IOptions<SubnauticaServerOptions> options = options;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
        await WaitForAvailablePortAsync(cts.Token);
    }

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task WaitForAvailablePortAsync(CancellationToken ct = default)
    {
        time.Restart();
        ushort port = options.Value.ServerPort;
        try
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                port = options.Value.ServerPort;
                if (!IsPortInUse(port))
                {
                    logger.ZLogTrace($"Port {port:@Port} UDP is available!");
                    break;
                }

                PrintPortWarn(logger, time, port);

                await Task.Delay(ITERATION_DELAY_MS, ct);
            }
        }
        catch (OperationCanceledException ex)
        {
            if (ex.CancellationToken.IsCancellationRequested)
            {
                logger.ZLogWarning($"Stopping the server as requested by user.");
            }
            else
            {
                logger.ZLogError($"Stopping the server as port {port:@Port} UDP is unavailable.");
            }
            throw;
        }

        static void PrintPortWarn(ILogger logger, Stopwatch sw, ushort port) =>
            logger.ZLogWarning($"Port {port:@Port} UDP is already in use. Please change the server port or close out any program that may be using it. Retrying for {int.Max(0, (int)Math.Ceiling(TIMEOUT_SECONDS - sw.Elapsed.TotalSeconds))}s...");
    }

    private bool IsPortInUse(ushort port)
    {
        foreach (IPEndPoint ip in IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners())
        {
            if (ip.Port == port)
            {
                return true;
            }
        }
        return false;
    }

}
