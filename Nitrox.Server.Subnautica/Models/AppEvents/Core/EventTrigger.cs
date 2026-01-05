namespace Nitrox.Server.Subnautica.Models.AppEvents.Core;

internal abstract class EventTrigger<TEventArgs>(Func<IEvent<TEventArgs>[]> handlers)
{
    protected readonly Lazy<IEvent<TEventArgs>[]> Handlers = new(handlers);
}
