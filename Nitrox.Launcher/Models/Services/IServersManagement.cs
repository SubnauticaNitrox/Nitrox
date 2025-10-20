using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MagicOnion.Server.Hubs;
using Nitrox.Launcher.Models.Design;
using Nitrox.Model.Logger;
using Nitrox.Model.MagicOnion;

namespace Nitrox.Launcher.Models.Services;

internal sealed class ServersManagement(ServerService serverService) : StreamingHubBase<IServersManagement, IServerManagementReceiver>, IServersManagement
{
    private readonly ServerService serverService = serverService;
    private readonly ConcurrentDictionary<Guid, int> connectionToProcessIdMap = [];

    public ValueTask SetPlayerCount(int processId, int playerCount)
    {
        ServerEntry? entry = serverService.GetServerEntryByAnyOf(processId);
        if (entry == null)
        {
            return CompletedTask;
        }
        entry.Players = playerCount;
        return CompletedTask;
    }

    public ValueTask AddOutputLine(int processId, string category, string time, int level, string message)
    {
        try
        {
            ServerEntry? entry = serverService.GetServerEntryByAnyOf(processId);
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
        int processId = await Client.OnRequestProcessId();
        ServerEntry? entry = serverService.GetServerEntryByAnyOf(processId, await Client.OnRequestSaveName());
        if (entry == null)
        {
            return;
        }
        if (processId > 0)
        {
            connectionToProcessIdMap[ConnectionId] = processId;
        }
        await entry.RefreshFromProcessAsync(processId);
    }

     protected override ValueTask OnDisconnected()
    {
        if (!connectionToProcessIdMap.TryRemove(ConnectionId, out int processId))
        {
            return CompletedTask;
        }
        ServerEntry? entry = serverService.GetServerEntryByAnyOf(processId);
        if (entry == null)
        {
            return CompletedTask;
        }
        entry.IsOnline = false;
        return CompletedTask;
    }
}
