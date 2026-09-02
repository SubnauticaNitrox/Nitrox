using System.Collections.Generic;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Players.Bans;

internal sealed class BanData
{
    public List<BanEntry> Bans { get; set; } = [];
}
