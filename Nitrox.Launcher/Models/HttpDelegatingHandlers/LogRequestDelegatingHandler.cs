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
            return await base.SendAsync(request, cancellationToken);
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
