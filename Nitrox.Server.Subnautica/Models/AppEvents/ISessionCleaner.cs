using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.AppEvents.Triggers;
using Nitrox.Server.Subnautica.Models.Communication;

namespace Nitrox.Server.Subnautica.Models.AppEvents;

internal interface ISessionCleaner : IEvent<ISessionCleaner.Args>
{
    public record Args(SessionManager.Session Session, int NewSessionTotal);

    public class Trigger(Func<IEvent<Args>[]> lazyHandlersProvider) : AsyncTrigger<Args>(lazyHandlersProvider);
}
