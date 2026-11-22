using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.Attributes;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

/// <summary>
///     These goals are unlocked individually (e.g. opening PDA, eating, picking up a fire extinguisher for the first time)
/// </summary>
[Name("PersonalCompletedGoalsWithTimestamp")]
internal sealed class CompletedGoalsWithTimestampProperty : IPlayerProperty<ThreadSafeDictionary<string, float>>
{
    public ThreadSafeDictionary<string, float> Value
    {
        get => Interlocked.CompareExchange(ref field, [], []);
        set => Interlocked.Exchange(ref field, value);
    } = [];
}
