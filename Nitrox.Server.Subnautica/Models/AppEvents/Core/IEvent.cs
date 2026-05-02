namespace Nitrox.Server.Subnautica.Models.AppEvents.Core;

internal interface IEvent<in TEventArgs>
{
    Task OnEventAsync(TEventArgs args);
}
