using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Attributes;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

[Name("CurrentStats")]
internal sealed class StatsProperty : IPlayerProperty<PlayerStatsData>
{
    public StatsProperty(IOptions<SubnauticaServerOptions> options)
    {
        SubnauticaServerOptions o = options.Value;
        Value = new PlayerStatsData(o.DefaultOxygenValue, o.DefaultMaxOxygenValue, o.DefaultHealthValue, o.DefaultHungerValue, o.DefaultThirstValue, o.DefaultInfectionValue);
    }

    public PlayerStatsData Value
    {
        get => Interlocked.CompareExchange(ref field, null, null);
        set => Interlocked.Exchange(ref field, value);
    }
}
