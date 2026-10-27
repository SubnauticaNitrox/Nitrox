namespace Nitrox.Server.Subnautica.Models.AppEvents.Core;

/// <summary>
///     Interface which is a blueprint for "event" based interfaces. Only singleton types are supported when implementing
///     this interface.
/// </summary>
/// <typeparam name="TEventArgs">The arguments that are passed to the event handlers when this event is triggered.</typeparam>
internal interface IEvent<in TEventArgs>
{
    Task OnEventAsync(TEventArgs args);
}
