namespace Nitrox.Server.Subnautica.Models.GameLogic.Players.Bans;

internal sealed class BanEntry
{
    public string IpAddress { get; set; }
    public string PlayerName { get; set; }
    public string Reason { get; set; } = "";
    public string BannedBy { get; set; } = "";
    public DateTimeOffset BannedAtUtc { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }

    public bool IsExpired => ExpiresAtUtc.HasValue && ExpiresAtUtc.Value <= DateTimeOffset.UtcNow;
}
