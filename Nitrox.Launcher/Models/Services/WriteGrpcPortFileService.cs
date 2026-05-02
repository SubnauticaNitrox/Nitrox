using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Nitrox.Model.Constants;

namespace Nitrox.Launcher.Models.Services;

/// <summary>
///     Writes the gRPC port in a file that Nitrox servers can access. This service is necessary because the listening port is dynamic to prevent "port in use" issues.
/// </summary>
internal class WriteGrpcPortFileService(IServer server) : IHostedLifecycleService
{
    private readonly string filePath = Path.Combine(Path.GetTempPath(), LauncherConstants.GRPC_LISTEN_PORT_TEMP_FILE_NAME);
    private readonly IServer server = server;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        IServerAddressesFeature? addressFeature = server.Features.Get<IServerAddressesFeature>();
        // We expect only one port to be known by .NET server (kestrel). If there are more, we need to refactor this code to ONLY write the gRPC port.
        int grpcPort = addressFeature.Addresses.Select(a => new Uri(a).Port).First(); // Should throw if more than one port.
        int attempts = 10;
        while (attempts-- > 0)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, grpcPort.ToString(), cancellationToken);
                break;
            }
            catch (Exception)
            {
                await Task.Delay(500, cancellationToken);
            }
        }
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (Exception)
        {
            // ignored
        }
        return Task.CompletedTask;
    }
}
