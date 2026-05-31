using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.AppEvents.Triggers;

namespace Nitrox.Server.Subnautica.Models.AppEvents;

internal interface IHibernate : IEvent<IHibernate.SleepArgs>, IEvent<IHibernate.WakeArgs>
{
    public record SleepArgs;

    public record WakeArgs;

    public class SleepTrigger(Func<IEvent<SleepArgs>[]> lazyHandlersProvider) : SequentialEmptyArgsTrigger<SleepArgs>(lazyHandlersProvider);

    public class WakeTrigger(Func<IEvent<WakeArgs>[]> lazyHandlersProvider) : SequentialEmptyArgsTrigger<WakeArgs>(lazyHandlersProvider);
}
