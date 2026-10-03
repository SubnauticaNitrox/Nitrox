using System.Net;
using Nitrox.Server.Subnautica.Models.Administration.Core;

namespace Nitrox.Server.Subnautica.Models.Administration;

internal interface IBan : IAdminFeature<IBan>
{
    /// <summary>
    ///     Returns whether the given IP address currently has an active (non-expired) ban.
    /// </summary>
    bool IsBanned(IPAddress ip);

    /// <summary>
    ///     Returns extra detail to show a banned player (the ban reason and, for a temporary ban, when it expires),
    ///     or <see langword="null" /> when the IP has no active ban.
    /// </summary>
    string? GetBanRejectionDetail(IPAddress ip);

    /// <summary>
    ///     Bans an IP address. The player name (if any) is only kept as a label for <c>banlist</c>; enforcement is purely
    ///     by IP so a rename or a different account behind the same IP stays banned.
    /// </summary>
    Task BanAsync(IPAddress ip, TimeSpan duration, string bannedBy, string? reason, string? playerName);

    Task<bool> UnbanAsync(IPAddress ip);
}
