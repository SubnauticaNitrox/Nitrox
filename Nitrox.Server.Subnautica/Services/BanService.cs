using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Server;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.GameLogic.Players.Bans;
using Nitrox.Server.Subnautica.Models.Serialization;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Tracks IP bans. Keeps its own save file (independent from <see cref="Serialization.World.WorldService" />'s
///     save/load cycle) so that old saves created before this feature existed keep loading fine.
/// </summary>
internal sealed class BanService(ServerJsonSerializer serializer, IOptions<ServerStartOptions> startOptions, ILogger<BanService> logger) : IHostedService, ISaveState
{
    private readonly ThreadSafeDictionary<string, BanEntry> bansByIp = [];
    private readonly TaskCompletionSource loaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly SemaphoreSlim expiryReschedule = new(0, 1);

    private CancellationTokenSource expiryLoopCancellation;
    private Task expiryLoopTask;

    private string FilePath => Path.Combine(startOptions.Value.GetServerSavePath(), $"Bans{serializer.FileEnding}");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(Load, cancellationToken);
        }
        finally
        {
            loaded.TrySetResult();
        }

        expiryLoopCancellation = new CancellationTokenSource();
        expiryLoopTask = RemoveExpiredBansLoopAsync(expiryLoopCancellation.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (expiryLoopCancellation is null)
        {
            return;
        }
        expiryLoopCancellation.Cancel();
        try
        {
            await expiryLoopTask;
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown.
        }
    }

    public async Task OnEventAsync(ISaveState.Args args)
    {
        await loaded.Task;
        Save();
    }

    /// <summary>
    ///     Returns whether the given IP address currently has an active (non-expired) ban.
    /// </summary>
    public bool IsBanned(IPAddress ip)
    {
        loaded.Task.Wait();
        return bansByIp.TryGetValue(Normalize(ip), out BanEntry entry) && !entry.IsExpired;
    }

    /// <summary>
    ///     Bans an IP address. The player name (if any) is only kept as a label for <c>banlist</c>; enforcement is purely
    ///     by IP so a rename or a different account behind the same IP stays banned.
    /// </summary>
    public async Task<BanEntry> BanAsync(IPAddress ip, string playerName, string reason, string bannedBy, TimeSpan? duration)
    {
        await loaded.Task;

        BanEntry entry = new()
        {
            IpAddress = Normalize(ip),
            PlayerName = playerName,
            Reason = reason ?? "",
            BannedBy = bannedBy ?? "",
            BannedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = duration.HasValue ? DateTimeOffset.UtcNow + duration.Value : null
        };
        bansByIp[entry.IpAddress] = entry;
        Save();
        SignalExpiryReschedule();
        return entry;
    }

    public async Task<bool> UnbanAsync(IPAddress ip)
    {
        await loaded.Task;

        if (!bansByIp.Remove(Normalize(ip)))
        {
            return false;
        }
        Save();
        return true;
    }

    public async Task<IReadOnlyList<BanEntry>> GetActiveBansAsync()
    {
        await loaded.Task;

        RemoveExpiredBans();
        return bansByIp.Values.ToList();
    }

    /// <summary>
    ///     Removes expired bans on startup (covers bans that lapsed while the server was offline) and then keeps sleeping
    ///     until the next ban is due to expire, waking early whenever a new ban is added.
    /// </summary>
    private async Task RemoveExpiredBansLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            RemoveExpiredBans();

            TimeSpan delay = TimeSpan.FromHours(1);
            List<DateTimeOffset> expiries = bansByIp.Values
                                                   .Where(entry => entry.ExpiresAtUtc.HasValue)
                                                   .Select(entry => entry.ExpiresAtUtc.Value)
                                                   .ToList();
            if (expiries.Count > 0)
            {
                TimeSpan untilNextExpiry = expiries.Min() - DateTimeOffset.UtcNow;
                if (untilNextExpiry < delay)
                {
                    delay = untilNextExpiry > TimeSpan.FromSeconds(1) ? untilNextExpiry : TimeSpan.FromSeconds(1);
                }
            }

            try
            {
                await expiryReschedule.WaitAsync(delay, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private void SignalExpiryReschedule()
    {
        try
        {
            expiryReschedule.Release();
        }
        catch (SemaphoreFullException)
        {
            // A reschedule is already queued.
        }
    }

    private void RemoveExpiredBans()
    {
        List<string> expiredKeys = bansByIp.Entries.Where(kv => kv.Value.IsExpired).Select(kv => kv.Key).ToList();
        if (expiredKeys.Count == 0)
        {
            return;
        }
        foreach (string key in expiredKeys)
        {
            bansByIp.Remove(key);
        }
        Save();
    }

    private void Load()
    {
        try
        {
            BanData data = serializer.Deserialize<BanData>(FilePath);
            foreach (BanEntry entry in data?.Bans ?? [])
            {
                bansByIp[entry.IpAddress] = entry;
            }
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            // No ban file yet (fresh save, or a save from before this feature existed).
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Could not load ban list, starting with an empty one");
        }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(startOptions.Value.GetServerSavePath());
            serializer.Serialize(FilePath, new BanData { Bans = bansByIp.Values.ToList() });
        }
        catch (Exception ex)
        {
            logger.ZLogError(ex, $"Could not save ban list");
        }
    }

    private static string Normalize(IPAddress ip) => ip.ToString();
}
