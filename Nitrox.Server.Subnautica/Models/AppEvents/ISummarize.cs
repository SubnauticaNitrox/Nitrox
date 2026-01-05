using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.AppEvents.Triggers;

namespace Nitrox.Server.Subnautica.Models.AppEvents;

/// <summary>
///     Implementations provide a user-friendly summary of their state.
/// </summary>
internal interface ISummarize : IEvent<ISummarize.Args>
{
    public record Args(Perms ViewerPerms);

    public class Trigger(Func<IEvent<Args>[]> lazyHandlersProvider) : SequentialTrigger<Args>(lazyHandlersProvider);
}
