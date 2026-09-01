using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HarmonyLib;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NitroxClient.Patching.Patches;

namespace NitroxClient.Services;

internal sealed class PatcherService(IEnumerable<IPersistentPatch> persistentPatches, IEnumerable<IDynamicPatch> dynamicPatches, ILogger<PatcherService> logger) : IHostedService
{
    private readonly IDynamicPatch[] dynamicPatches = dynamicPatches.ToArray();
    private readonly ILogger<PatcherService> logger = logger;
    private readonly IPersistentPatch[] persistentPatches = persistentPatches.ToArray();
    private Harmony harmony;
    private bool isApplied;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        harmony = new("com.nitroxmod.harmony");
        foreach (IPersistentPatch patch in persistentPatches)
        {
            logger.LogDebug($"Applying persistent patch {patch.GetType().Name}");
            patch.Patch(harmony);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Apply()
    {
        if (isApplied)
        {
            return;
        }

        foreach (IDynamicPatch patch in dynamicPatches)
        {
            logger.LogDebug($"Applying dynamic patch {patch.GetType().Name}");
            try
            {
                patch.Patch(harmony);
            }
            catch (HarmonyException e)
            {
                Exception innerMost = e;
                while (innerMost.InnerException != null)
                {
                    innerMost = innerMost.InnerException;
                }
                logger.LogError($"Error patching {patch.GetType().Name}{Environment.NewLine}{innerMost}");
            }
            catch (Exception e)
            {
                logger.LogError($"Error patching {patch.GetType().Name}{Environment.NewLine}{e}");
            }
        }

        isApplied = true;
    }

    /// <summary>
    ///     Removes all the dynamic patches defined by <see cref="NitroxClient" />.
    ///     <p />
    ///     If the player starts the main menu for the first time, or returns from a (multiplayer) session, get rid of all the
    ///     patches if applicable.
    /// </summary>
    public void Restore()
    {
        if (!isApplied)
        {
            return;
        }

        foreach (IDynamicPatch patch in dynamicPatches)
        {
            logger.LogDebug($"Restoring dynamic patch {patch.GetType().Name}");
            patch.Restore(harmony);
        }

        isApplied = false;
    }
}
