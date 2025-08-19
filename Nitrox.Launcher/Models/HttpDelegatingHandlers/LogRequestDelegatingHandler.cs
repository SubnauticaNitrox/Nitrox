using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Launcher.Models.Utils;
using NitroxModel.Helper;
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
            LogResponseStatus(response, url);
            if (IsErrorStatus(response.StatusCode))
            {
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
            LauncherNotifier.Error(NetHelper.HasInternetConnectivityAsync() ? ex.Message : "No internet connection available");
            throw;
        }

        static void LogResponseStatus(HttpResponseMessage response, string url)
        {
            string message = $"HTTP response status:{response.StatusCode} from {url}";
            if (IsErrorStatus(response.StatusCode))
            {
                Log.Error(message);
            }
            else
            {
                Log.Debug(message);
            }
        }

        static bool IsErrorStatus(HttpStatusCode statusCode) => (int)statusCode is < 200 or >= 400;
    }
}
