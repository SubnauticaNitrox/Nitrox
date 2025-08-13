using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Launcher.Models.Utils;
using NitroxModel.Logger;

namespace Nitrox.Launcher.Models.HttpDelegatingHandlers;

internal sealed class LogRequestDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string url = request.RequestUri == null ? "<no-url>" : request.RequestUri.ToString();
        Log.Info($"{request.Method} request to {url}");
        try
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            if ((int)response.StatusCode is >= 200 and < 400)
            {
                Log.Debug($"HTTP response status:{response.StatusCode} from {url}");
            }
            else
            {
                Log.Error($"HTTP response status:{response.StatusCode} from {url}");
                LauncherNotifier.Error($"Failed to fetch data from {request.RequestUri?.Host}");
            }
            return response;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LauncherNotifier.Error(ex.Message);
            throw;
        }
    }
}
