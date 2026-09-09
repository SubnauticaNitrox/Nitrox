using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.Serialization;
using Nitrox.Server.Subnautica.Models.Serialization.World;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Tracks IP bans. Keeps its own save file (independent from <see cref="WorldService" />'s
///     save/load cycle) so that old saves created before this feature existed keep loading fine.
/// </summary>
internal sealed class BanService(ServerJsonSerializer serializer, IOptions<ServerStartOptions> startOptions, ILogger<BanService> logger) : BackgroundService, ISaveState
{
    private readonly ThreadSafeDictionary<IPAddress, BanEntry> bansByIp = [];
    private readonly SemaphoreSlim expiryReschedule = new(0, 1);
    private readonly TaskCompletionSource loaded = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private string FilePath => Path.Combine(startOptions.Value.GetServerSavePath(), $"Bans{serializer.FileEnding}");

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(Load, cancellationToken);
        }
        finally
        {
            loaded.TrySetResult();
        }
        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    ///     Returns whether the given IP address currently has an active (non-expired) ban.
    /// </summary>
    public bool IsBanned(IPAddress ip)
    {
        loaded.Task.GetAwaiter().GetResult();
        return bansByIp.TryGetValue(ip, out BanEntry? entry) && !entry.IsExpired;
    }

    /// <summary>
    ///     Returns extra detail to show a banned player (the ban reason and, for a temporary ban, when it expires),
    ///     or <see langword="null" /> when the IP has no active ban.
    /// </summary>
    public string? GetBanRejectionDetail(IPAddress ip)
    {
        loaded.Task.GetAwaiter().GetResult();
        if (!bansByIp.TryGetValue(ip, out BanEntry? entry) || entry.IsExpired)
        {
            return null;
        }

        List<string> lines = [];
        if (!string.IsNullOrWhiteSpace(entry.Reason))
        {
            lines.Add($"Reason: {entry.Reason}");
        }
        lines.Add(entry.ExpiresAtUtc.HasValue
                      ? $"Ban expires {entry.ExpiresAtUtc.Value.UtcDateTime:yyyy-MM-dd HH:mm} UTC"
                      : "This ban is permanent.");
        return string.Join("\n", lines);
    }

    /// <summary>
    ///     Bans an IP address. The player name (if any) is only kept as a label for <c>banlist</c>; enforcement is purely
    ///     by IP so a rename or a different account behind the same IP stays banned.
    /// </summary>
    public async Task BanAsync(IPAddress ip, string? playerName, string? reason, string bannedBy, TimeSpan duration)
    {
        await loaded.Task;

        DateTimeOffset bannedAtTime = DateTimeOffset.UtcNow;
        BanEntry entry = new()
        {
            IP = ip,
            PlayerName = playerName,
            Reason = reason ?? "",
            BannedBy = bannedBy,
            BannedAtUtc = bannedAtTime,
            ExpiresAtUtc = duration == TimeSpan.Zero ? null : bannedAtTime + duration
        };
        bansByIp[entry.IP] = entry;
        SignalExpiryReschedule();
    }

    public async Task<bool> UnbanAsync(IPAddress ip)
    {
        await loaded.Task;

        if (!bansByIp.Remove(ip))
        {
            return false;
        }
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
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            RemoveExpiredBans();

            TimeSpan delay = TimeSpan.FromHours(1);
            DateTimeOffset[] expiredBans = bansByIp.Values
                                                   .Where(entry => entry.ExpiresAtUtc.HasValue)
                                                   .Select(entry => entry.ExpiresAtUtc.Value)
                                                   .ToArray();
            if (expiredBans.Length > 0)
            {
                TimeSpan untilNextExpiry = expiredBans.Min() - DateTimeOffset.UtcNow;
                if (untilNextExpiry < delay)
                {
                    delay = untilNextExpiry > TimeSpan.FromSeconds(1) ? untilNextExpiry : TimeSpan.FromSeconds(1);
                }
            }

            try
            {
                await expiryReschedule.WaitAsync(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    async Task IEvent<ISaveState.Args>.OnEventAsync(ISaveState.Args args)
    {
        await loaded.Task;
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

    private void SignalExpiryReschedule()
    {
        try
        {
            expiryReschedule.Release();
        }
        catch (SemaphoreFullException)
        {
            // A rescheduling is already queued.
        }
    }

    private void RemoveExpiredBans()
    {
        foreach (BanEntry entry in bansByIp.Values.Where(kv => kv.IsExpired))
        {
            bansByIp.Remove(entry.IP);
            logger.ZLogDebug($"A ban expired: {entry}");
        }
    }

    private void Load()
    {
        try
        {
            BanData? data = serializer.Deserialize<BanData>(FilePath);
            foreach (BanEntry entry in data?.Bans ?? [])
            {
                bansByIp[entry.IP] = entry;
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

    private sealed class BanData
    {
        public List<BanEntry> Bans { get; init; } = [];
    }

    internal sealed record BanEntry
    {
        [IgnoreDataMember]
        public IPAddress IP { get; set; } = IPAddress.None;

        [JsonProperty(nameof(IP))]
        private string JsonIP
        {
            get => IP.ToString();
            set => IP = IPAddress.Parse(value);
        }

        public string? PlayerName { get; init; }
        public string Reason { get; init; } = "";
        public string BannedBy { get; init; } = "";
        public DateTimeOffset BannedAtUtc { get; init; }
        public DateTimeOffset? ExpiresAtUtc { get; init; }

        [IgnoreDataMember]
        public bool IsExpired => ExpiresAtUtc.HasValue && ExpiresAtUtc.Value <= DateTimeOffset.UtcNow;

        public override string ToString() => $"[{nameof(PlayerName)}: {(string.IsNullOrEmpty(PlayerName) ? "<empty>" : PlayerName)}, {nameof(IP)}: {IP}, {nameof(BannedBy)}: {BannedBy}, {nameof(BannedAtUtc)}: {BannedAtUtc}, {nameof(Reason)}: {Reason}]";
    }
}
