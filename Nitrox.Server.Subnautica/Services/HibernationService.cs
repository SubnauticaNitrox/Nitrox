using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Hibernation;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which can signal hibernating services to <see cref="Resume" /> or <see cref="Hibernate" />.
/// </summary>
internal sealed class HibernationService(IEnumerable<IHibernate> hibernators, ILogger<HibernationService> logger) : IHostedService
{
    private readonly IHibernate[] hibernators = [..hibernators];
    private readonly ILogger<HibernationService> logger = logger;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (IHibernate hibernator in hibernators)
        {
            logger.LogTrace("Added hibernator {TypeName}", hibernator.GetType().Name);
        }
        logger.LogDebug("{HibernateCount} hibernators found and registered", hibernators.Length);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Hibernate()
    {
        foreach (IHibernate hibernator in hibernators)
        {
            hibernator.Hibernate();
        }
    }

    public void Resume()
    {
        foreach (IHibernate hibernator in hibernators)
        {
            hibernator.Resume();
        }
    }
}
