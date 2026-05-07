using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using LiteNetLib;
using LiteNetLib.Utils;
using Nitrox.Launcher.Models.Design;
using Nitrox.Model.Constants;
using Nitrox.Model.Core;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using Nitrox.Model.Networking;
using Nitrox.Model.Serialization;

namespace Nitrox.Launcher.Models.Services;

internal sealed class RecentServerStatusService
{
    private const string RECENT_SERVERS_CACHE_FILE_NAME = "launcher_recent_servers.cache";
    private const int NETWORK_POLL_DELAY_MS = 25;
    private const int REMOTE_STATUS_TIMEOUT_SECONDS = 2;
    private const int MAX_CONCURRENT_QUERIES = 5;
    private static readonly TimeSpan autoRefreshInterval = TimeSpan.FromSeconds(15);
    private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private CancellationTokenSource? autoRefreshCts;
    private int autoRefreshCycleCounter;
    private volatile bool isRefreshInProgress;
    private List<string> cachedOnlineServersEndpoints = [];
    private bool hasInitializedOnlineServersState;

    /// <summary>
    /// Returns a list of RecentServerEntries based on the current entries in ServerList, enriched with cached data from previous status queries (if available).
    /// </summary>
    public AvaloniaList<RecentServerEntry> GetRecentServers()
    {
        RecentServersCacheData cacheData = LoadCache();
        Dictionary<string, RecentServerCacheEntry> cacheByEndpoint = cacheData.Servers
                                                                              .Where(entry => !string.IsNullOrWhiteSpace(entry.Address))
                                                                              .ToDictionary(GetEndpointKey, StringComparer.OrdinalIgnoreCase);

        AvaloniaList<RecentServerEntry> list = [];
        ServerList.Refresh();
        foreach (ServerList.Entry entry in ServerList.Instance.Entries)
        {
            string endpointKey = GetEndpointKey(entry.Address, entry.Port);
            cacheByEndpoint.TryGetValue(endpointKey, out RecentServerCacheEntry? cached);

            RecentServerEntry recentEntry = new()
            {
                LocalServerName = entry.Name,
                RemoteHostServerName = cached?.RemoteHostServerName,
                ServerIP = entry.Address,
                ServerPort = entry.Port,
                MaxPlayers = cached?.MaxPlayers ?? 100,
                ServerIcon = RecentServerEntry.DefaultServerIcon,
                IsStatusLoading = false,
                PlayerNames = []
            };

            list.Add(recentEntry);
        }

        return list;
    }

    public async Task RefreshRecentServersAsync(AvaloniaList<RecentServerEntry> recentServers, bool prioritizeOnlineFirst = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recentServers);

        await Task.Run(() => RefreshRecentServersCoreAsync(recentServers, prioritizeOnlineFirst, cancellationToken), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Refreshes the status of the provided list of RecentServerEntries by querying each server for its current status, and updates the entries accordingly.
    /// </summary>
    private async Task RefreshRecentServersCoreAsync(AvaloniaList<RecentServerEntry> recentServers, bool prioritizeOnlineFirst, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recentServers);

        isRefreshInProgress = true;
        try
        {
            foreach (RecentServerEntry server in recentServers)
            {
                server.IsStatusLoading = true;
                server.IsVersionCompatible = true;
            }

            RecentServersCacheData cacheData = LoadCache();
            ConcurrentDictionary<string, RecentServerCacheEntry> cacheByEndpoint = new(cacheData.Servers
                                                                                             .Where(entry => !string.IsNullOrWhiteSpace(entry.Address))
                                                                                             .ToDictionary(GetEndpointKey, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

            IEnumerable<RecentServerEntry> refreshOrder = prioritizeOnlineFirst
                ? recentServers.OrderByDescending(server => server.IsOnline)
                : recentServers;

            SemaphoreSlim semaphore = new(MAX_CONCURRENT_QUERIES);
            try
            {
                List<Task> refreshTasks = [];
                int cacheChanged = 0;
                Action markCacheChanged = () => Interlocked.Exchange(ref cacheChanged, 1);

                refreshTasks.AddRange(refreshOrder.ToArray().Select(server => RefreshSingleServerAsync(server, cacheByEndpoint, semaphore, markCacheChanged, cancellationToken)));

                await Task.WhenAll(refreshTasks).ConfigureAwait(false);

                if (cacheChanged != 0)
                {
                    cacheData.Servers = [..cacheByEndpoint.Values.OrderBy(entry => entry.Address).ThenBy(entry => entry.Port)];
                    SaveCache(cacheData);
                }
            }
            finally
            {
                semaphore.Dispose();
            }
        }
        finally
        {
            isRefreshInProgress = false;
        }
    }

    /// <summary>
    /// Queries a specific RecentServerEntry and updates its values based on the query result. Also updates the cache entry for this server and marks it as changed if needed.
    /// </summary>
    private async Task RefreshSingleServerAsync(RecentServerEntry server, ConcurrentDictionary<string, RecentServerCacheEntry> cacheByEndpoint, SemaphoreSlim semaphore, Action markCacheChanged, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            RemoteServerStatusResponse? status = await QueryServerStatusAsync(server.ServerIP, server.ServerPort, cancellationToken).ConfigureAwait(false);
            if (status == null)
            {
                server.IsOnline = false;
                server.PlayerCount = 0;
                server.PlayerNames = [];
                return;
            }

            string? remoteHostName = string.IsNullOrWhiteSpace(status.HostServerName) ? status.ServerName : status.HostServerName;
            server.IsOnline = status.IsOnline;
            server.NitroxVersion = status.NitroxVersion;
            server.IsVersionCompatible = string.Equals(status.NitroxVersion, NitroxEnvironment.Version.ToString(), StringComparison.Ordinal);
            server.PlayerCount = status.PlayerCount;
            server.MaxPlayers = status.MaxPlayerCount > 0 ? status.MaxPlayerCount : server.MaxPlayers;
            server.PlayerNames = [..status.PlayerNames];
            if (!string.IsNullOrWhiteSpace(remoteHostName))
            {
                server.RemoteHostServerName = remoteHostName;
            }

            string endpointKey = GetEndpointKey(server.ServerIP, server.ServerPort);
            RecentServerCacheEntry cacheEntry = cacheByEndpoint.GetOrAdd(endpointKey, _ => new RecentServerCacheEntry { Address = server.ServerIP, Port = server.ServerPort });

            // TODO: Add support for server icon retrieval
            server.ServerIcon = RecentServerEntry.DefaultServerIcon;

            string newRemoteName = remoteHostName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(newRemoteName) && cacheEntry.RemoteHostServerName != null)
            {
                newRemoteName = cacheEntry.RemoteHostServerName;
            }

            int newMaxPlayers = status.MaxPlayerCount > 0 ? status.MaxPlayerCount : cacheEntry.MaxPlayers;
            if (!string.Equals(cacheEntry.RemoteHostServerName, newRemoteName, StringComparison.Ordinal) || cacheEntry.MaxPlayers != newMaxPlayers)
            {
                cacheEntry.RemoteHostServerName = newRemoteName;
                cacheEntry.MaxPlayers = newMaxPlayers;
                markCacheChanged();
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to refresh recent server status for {server.ServerIP}:{server.ServerPort}");
        }
        finally
        {
            server.IsStatusLoading = false;
            semaphore.Release();
        }
    }

    /// <summary>
    /// Starts an auto-refresh loop that periodically refreshes the status of the RecentServerEntries.
    /// </summary>
    /// <remarks>
    /// Refreshes only online RecentServerEntries every <see cref="autoRefreshInterval"/>, and then refreshes all of them every 4th cycle
    /// </remarks>
    public void StartAutoRefresh(AvaloniaList<RecentServerEntry> recentServers, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(recentServers);

        try
        {
            autoRefreshCts?.Cancel();
            autoRefreshCts?.Dispose();
            autoRefreshCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            CancellationToken token = autoRefreshCts.Token;

            autoRefreshCycleCounter = 0;

            _ = Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        autoRefreshCycleCounter++;

                        if (autoRefreshCycleCounter % 4 == 0)
                        {
                            if (!isRefreshInProgress)
                            {
                                await RefreshRecentServersCoreAsync(recentServers, prioritizeOnlineFirst: false, token).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            if (!isRefreshInProgress)
                            {
                                AvaloniaList<RecentServerEntry> onlineServers = [];
                                foreach (RecentServerEntry entry in recentServers)
                                {
                                    if (entry.IsOnline)
                                    {
                                        onlineServers.Add(entry);
                                    }
                                }

                                if (onlineServers.Count > 0)
                                {
                                    await RefreshRecentServersCoreAsync(onlineServers, prioritizeOnlineFirst: true, token).ConfigureAwait(false);
                                }
                            }
                        }

                        await Task.Delay(autoRefreshInterval, token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Log.Error(ex, "Auto-refresh loop failed");
                }
            }, token);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start auto-refresh loop");
        }
    }

    public void StopAutoRefresh()
    {
        try
        {
            autoRefreshCts?.Cancel();
            autoRefreshCts?.Dispose();
            autoRefreshCts = null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to stop auto-refresh loop");
        }
    }

    public void SaveOnlineServersState(AvaloniaList<RecentServerEntry> recentServers)
    {
        ArgumentNullException.ThrowIfNull(recentServers);

        cachedOnlineServersEndpoints.Clear();
        foreach (RecentServerEntry server in recentServers)
        {
            if (server.IsOnline)
            {
                cachedOnlineServersEndpoints.Add(GetEndpointKey(server.ServerIP, server.ServerPort));
            }
        }

        hasInitializedOnlineServersState = true;
    }

    /// <summary>
    /// Returns a list of RecentServerEntries that should be refreshed based when LibraryView is loaded.
    /// </summary>
    /// <remarks>
    /// On first load: Return all the RecentServerEntries.
    /// On subsequent loads: Return only the RecentServerEntries that were previously online, based on <see cref="cachedOnlineServersEndpoints"/>.
    /// </remarks>
    public AvaloniaList<RecentServerEntry> GetServersToRefreshOnLoad(AvaloniaList<RecentServerEntry> recentServers)
    {
        ArgumentNullException.ThrowIfNull(recentServers);

        // First time load
        if (!hasInitializedOnlineServersState)
        {
            return recentServers;
        }

        if (cachedOnlineServersEndpoints.Count == 0)
        {
            return [];
        }

        HashSet<string> previouslyOnlineSet = new(cachedOnlineServersEndpoints, StringComparer.OrdinalIgnoreCase);

        AvaloniaList<RecentServerEntry> serversToRefresh = new();
        foreach (RecentServerEntry server in recentServers)
        {
            string endpointKey = GetEndpointKey(server.ServerIP, server.ServerPort);
            if (previouslyOnlineSet.Contains(endpointKey))
            {
                serversToRefresh.Add(server);
            }
        }

        return serversToRefresh;
    }

    /// <summary>
    /// Queries and returns the status of a RecentServerEntry based on <paramref name="serverIp"/> and <paramref name="serverPort"/>
    /// </summary>
    private static async Task<RemoteServerStatusResponse?> QueryServerStatusAsync(string serverIp, int serverPort, CancellationToken cancellationToken)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(REMOTE_STATUS_TIMEOUT_SECONDS);
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        EventBasedNetListener listener = new();
        NetManager client = new(listener)
        {
            AutoRecycle = true,
            UnconnectedMessagesEnabled = true,
            IPv6Enabled = true
        };

        try
        {
            if (!client.Start())
            {
                return null;
            }

            TaskCompletionSource<RemoteServerStatusResponse?> completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            listener.NetworkReceiveUnconnectedEvent += ReceiveUnconnected;

            NetDataWriter writer = new();
            writer.Put(RemoteServerStatusConstants.REQUEST_STRING);
            client.SendUnconnectedMessage(writer, serverIp, serverPort);

            while (!completionSource.Task.IsCompleted && !timeoutCts.IsCancellationRequested)
            {
                client.PollEvents();
                await Task.Delay(NETWORK_POLL_DELAY_MS, timeoutCts.Token).ConfigureAwait(false);
            }

            if (!completionSource.Task.IsCompleted)
            {
                return null;
            }

            return await completionSource.Task;

            void ReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
            {
                if (messageType != UnconnectedMessageType.BasicMessage || remoteEndPoint.Port != serverPort)
                {
                    return;
                }

                try
                {
                    string responseType = reader.GetString();
                    if (responseType != RemoteServerStatusConstants.RESPONSE_STRING)
                    {
                        return;
                    }

                    string payload = reader.GetString();
                    RemoteServerStatusResponse? response = JsonSerializer.Deserialize<RemoteServerStatusResponse>(payload, jsonSerializerOptions);
                    completionSource.TrySetResult(response);
                }
                catch
                {
                    completionSource.TrySetResult(null);
                }
            }
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch
        {
            return null;
        }
        finally
        {
            listener.ClearNetworkReceiveUnconnectedEvent();
            client.Stop();
        }
    }

    private static string GetCachePath() => Path.Combine(NitroxUser.CachePath, RECENT_SERVERS_CACHE_FILE_NAME);

    private static string GetEndpointKey(RecentServerCacheEntry entry) => GetEndpointKey(entry.Address, entry.Port);

    private static string GetEndpointKey(string address, int port) => $"{address.Trim()}:{port}";

    private static RecentServersCacheData LoadCache()
    {
        try
        {
            string path = GetCachePath();
            if (!File.Exists(path))
            {
                return new RecentServersCacheData();
            }

            string serialized = File.ReadAllText(path);
            return JsonSerializer.Deserialize<RecentServersCacheData>(serialized, jsonSerializerOptions) ?? new RecentServersCacheData();
        }
        catch
        {
            return new RecentServersCacheData();
        }
    }

    private static void SaveCache(RecentServersCacheData data)
    {
        Directory.CreateDirectory(NitroxUser.CachePath);
        string serialized = JsonSerializer.Serialize(data, jsonSerializerOptions);
        File.WriteAllText(GetCachePath(), serialized);
    }

    private sealed class RecentServersCacheData
    {
        public List<RecentServerCacheEntry> Servers { get; set; } = [];
    }

    private sealed class RecentServerCacheEntry
    {
        public string Address { get; set; } = string.Empty;

        public int Port { get; set; } = SubnauticaServerConstants.DEFAULT_PORT;

        public string? RemoteHostServerName { get; set; }

        public int MaxPlayers { get; set; } = SubnauticaServerConstants.DEFAULT_MAX_PLAYERS;
    }
}
