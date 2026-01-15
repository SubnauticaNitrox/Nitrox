using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Server.Hubs;
using Nitrox.Launcher.Models.Design;
using Nitrox.Model.Logger;
using Nitrox.Model.MagicOnion;

namespace Nitrox.Launcher.Models.Services;

/// <summary>
///     Scoped service (i.e. instance for each server) that handles server management (like sending commands and
///     receiving output).
/// </summary>
internal sealed class ServersManagement(ServerService serverService) : StreamingHubBase<IServersManagement, IServerManagementReceiver>, IServersManagement
{
    private readonly CancellationTokenSource cts = new();
    private readonly ServerService serverService = serverService;
    private int processId;
    private string saveName;

    public ValueTask SetPlayerCount(int playerCount)
    {
        ServerEntry? entry = serverService.GetServerEntryByAnyOf(processId, saveName);
        if (entry == null)
        {
            return CompletedTask;
        }
        entry.Players = playerCount;
        return CompletedTask;
    }

    public ValueTask AddOutputLine(string category, string time, int level, string message)
    {
        try
        {
            ServerEntry? entry = serverService.GetServerEntryByAnyOf(processId, saveName);
            entry?.Output.Add(new OutputLine
            {
                LogText = message,
                Timestamp = time,
                Type = (OutputLineType)level
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex);
            throw;
        }
        return CompletedTask;
    }

    protected override async ValueTask OnConnected()
    {
        Metadata headers = Context.CallContext.RequestHeaders;
        if (!int.TryParse(headers.GetValue("ProcessId"), out processId))
        {
            return;
        }
        saveName = headers.GetValue("SaveName") ?? "";
        if (saveName == "")
        {
            return;
        }
        ServerEntry? entry = await WaitForEntryAsync(cts.Token);
        if (entry == null)
        {
            return;
        }
        await entry.RefreshFromProcessAsync(processId);
        _ = HandleCommandLoopAsync().ContinueWithHandleError();
    }

    protected override async ValueTask OnDisconnected()
    {
        ServerEntry? entry = serverService.GetServerEntryByAnyOf(processId, saveName);
        entry?.ResetCts();
        processId = 0;
        saveName = "";
        await cts.CancelAsync();
    }

    private async Task<ServerEntry?> WaitForEntryAsync(CancellationToken cancellationToken)
    {
        try
        {
            do
            {
                ServerEntry? entry = serverService.GetServerEntryByAnyOf(processId, saveName);
                if (entry != null)
                {
                    return entry;
                }
                await Task.Delay(250, cancellationToken);
            } while (!cancellationToken.IsCancellationRequested);
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        return null;
    }

    private async Task HandleCommandLoopAsync()
    {
        ServerEntry? entry = serverService.GetServerEntryByAnyOf(processId, saveName);
        if (entry == null)
        {
            return;
        }
        await foreach (string command in entry.CommandQueue.Reader.ReadAllAsync())
        {
            Client.OnCommand(command);
        }
    }
}
