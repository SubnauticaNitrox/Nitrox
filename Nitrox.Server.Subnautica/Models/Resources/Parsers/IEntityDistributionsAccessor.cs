using System.Threading;
using System.Threading.Tasks;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

/// <summary>
///     Abstraction for tests and <see cref="SubnauticaUwePrefabFactory" /> to consume loot distribution data without coupling to <see cref="EntityDistributionsResource" />.
/// </summary>
internal interface IEntityDistributionsAccessor
{
    Task<LootDistributionData> GetLootDistributionDataAsync(CancellationToken cancellationToken = default);
}
