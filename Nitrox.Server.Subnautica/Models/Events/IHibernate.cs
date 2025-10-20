namespace Nitrox.Server.Subnautica.Models.Events;

internal interface IHibernate
{
    Task SleepAsync();

    Task WakeAsync();
}
