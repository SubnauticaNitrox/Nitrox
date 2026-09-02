using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Server;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.Serialization;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Players.Bans;

/// <summary>
///     Tracks IP bans. Keeps its own save file (independent from <see cref="Serialization.World.WorldService" />'s
///     save/load cycle) so that old saves created before this feature existed keep loading fine.
/// </summary>
internal sealed class BanManager(ServerJsonSerializer serializer, IOptions<ServerStartOptions> startOptions, ILogger<BanManager> logger) : IHostedService, ISaveState
{
    private readonly ThreadSafeDictionary<string, BanEntry> bansByIp = [];

    private string FilePath => Path.Combine(startOptions.Value.GetServerSavePath(), $"Bans{serializer.FileEnding}");

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Load();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task OnEventAsync(ISaveState.Args args)
    {
        Save();
        return Task.CompletedTask;
    }

    public bool TryGetBan(IPAddress ip, out BanEntry entry)
    {
        string key = Normalize(ip);
        if (!bansByIp.TryGetValue(key, out entry))
        {
            return false;
        }
        if (!entry.IsExpired)
        {
            return true;
        }
        bansByIp.Remove(key);
        Save();
        entry = null;
        return false;
    }

    public BanEntry Ban(IPAddress ip, string playerName, string reason, string bannedBy, TimeSpan? duration)
    {
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
        return entry;
    }

    public bool Unban(string ipOrPlayerName)
    {
        string key = IPAddress.TryParse(ipOrPlayerName, out IPAddress ip)
            ? Normalize(ip)
            : bansByIp.Entries.FirstOrDefault(kv => string.Equals(kv.Value.PlayerName, ipOrPlayerName, StringComparison.OrdinalIgnoreCase)).Key;

        if (key == null || !bansByIp.Remove(key))
        {
            return false;
        }
        Save();
        return true;
    }

    public IReadOnlyList<BanEntry> GetActiveBans()
    {
        List<string> expiredKeys = bansByIp.Entries.Where(kv => kv.Value.IsExpired).Select(kv => kv.Key).ToList();
        if (expiredKeys.Count > 0)
        {
            foreach (string key in expiredKeys)
            {
                bansByIp.Remove(key);
            }
            Save();
        }
        return bansByIp.Values.ToList();
    }

    private void Load()
    {
        string filePath = FilePath;
        if (!File.Exists(filePath))
        {
            return;
        }
        try
        {
            BanData data = serializer.Deserialize<BanData>(filePath);
            foreach (BanEntry entry in data?.Bans ?? [])
            {
                bansByIp[entry.IpAddress] = entry;
            }
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
