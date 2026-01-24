using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.AppEvents.Triggers;

namespace Nitrox.Server.Subnautica.Models.AppEvents;

/// <summary>
///     Event to let other services save their state to the same directory as the server save.
/// </summary>
internal interface ISaveState : IEvent<ISaveState.Args>
{
    /// <param name="SavePath">Path to the save directory of the current game server instance.</param>
    public record Args(string SavePath);

    public class Trigger(Func<IEvent<Args>[]> lazyHandlersProvider) : AsyncTrigger<Args>(lazyHandlersProvider);
}
